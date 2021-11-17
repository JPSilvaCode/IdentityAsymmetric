﻿using IA.Identity.API.Data;
using IA.Identity.API.Extensions;
using IA.Identity.API.Identity;
using IA.Identity.API.Models;
using IA.WebAPI.Core.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using IA.WebAPI.Core.User;
using NetDevPack.Security.Jwt.Interfaces;

namespace IA.Identity.API.Services.v3_0
{
    public class AuthenticationService
    {
        public readonly SignInManager<ApplicationUser> SignInManager;
        public readonly UserManager<ApplicationUser> UserManager;
        private readonly AppSettings _appSettings;
        private readonly AppTokenSettings _appTokenSettingsSettings;
        private readonly IAContext _context;

        private readonly IJsonWebKeySetService _jwksService;
        private readonly IAspNetUser _aspNetUser;

        public AuthenticationService(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IOptions<AppSettings> appSettings,
            IOptions<AppTokenSettings> appTokenSettingsSettings,
            IAContext context,
            IJsonWebKeySetService jwksService,
            IAspNetUser aspNetUser)
        {
            SignInManager = signInManager;
            UserManager = userManager;
            _appSettings = appSettings.Value;
            _appTokenSettingsSettings = appTokenSettingsSettings.Value;
            _jwksService = jwksService;
            _aspNetUser = aspNetUser;
            _context = context;
        }

        public async Task<UserResponseLogin> CreateJwt(string email)
        {
            var user = await UserManager.FindByEmailAsync(email);
            var claims = await UserManager.GetClaimsAsync(user);

            var identityClaims = await GetClaimsUsuario(claims, user);
            var encodedToken = EncodeToken(identityClaims);

            var refreshToken = await CreateRefreshToken(email);

            return GetResponseToken(encodedToken, user, claims, refreshToken);
        }

        private async Task<ClaimsIdentity> GetClaimsUsuario(ICollection<Claim> claims, ApplicationUser user)
        {
            var userRoles = await UserManager.GetRolesAsync(user);

            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, ToUnixEpochDate(DateTime.UtcNow).ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(DateTime.UtcNow).ToString(),
                ClaimValueTypes.Integer64));
            foreach (var userRole in userRoles)
            {
                claims.Add(new Claim("role", userRole));
            }

            var identityClaims = new ClaimsIdentity();
            identityClaims.AddClaims(claims);

            return identityClaims;
        }

        private string EncodeToken(ClaimsIdentity identityClaims)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var currentIssuer = $"{_aspNetUser.GetHttpContext().Request.Scheme}://{_aspNetUser.GetHttpContext().Request.Host}";
            var key = _jwksService.GetCurrentSigningCredentials();
            var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = currentIssuer,
                Subject = identityClaims,
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = key
            });

            return tokenHandler.WriteToken(token);
        }

        private UserResponseLogin GetResponseToken(string encodedToken, ApplicationUser user, IEnumerable<Claim> claims, RefreshToken refreshToken)
        {
            return new()
            {
                AccessToken = encodedToken,
                RefreshToken = refreshToken.Token,
                ExpiresIn = TimeSpan.FromHours(1).TotalSeconds,
                UsuarioToken = new UserToken
                {
                    Id = user.Id,
                    Email = user.Email,
                    Claims = claims.Select(c => new UserClaim { Type = c.Type, Value = c.Value })
                }
            };
        }

        private static long ToUnixEpochDate(DateTime date) => (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);

        private async Task<RefreshToken> CreateRefreshToken(string email)
        {
            var refreshToken = new RefreshToken
            {
                UserEmail = email,
                ExpirationDate = DateTime.UtcNow.AddHours(_appTokenSettingsSettings.RefreshTokenExpiration)
            };

            _context.RefreshTokens.RemoveRange(_context.RefreshTokens.Where(u => u.UserEmail == email));
            await _context.RefreshTokens.AddAsync(refreshToken);

            await _context.SaveChangesAsync();

            return refreshToken;
        }

        public async Task<RefreshToken> GetRefreshToken(Guid refreshToken)
        {
            var token = await _context.RefreshTokens.AsNoTracking().FirstOrDefaultAsync(u => u.Token == refreshToken);

            return token != null && token.ExpirationDate.ToLocalTime() > DateTime.Now ? token : null;
        }
    }
}