﻿using IA.Identity.API.Data;
using IA.Identity.API.Extensions;
using IA.Identity.API.Identity;
using IA.Identity.API.Models;
using IA.WebAPI.Core.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NetDevPack.Security.Jwt.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IA.Identity.API.Services.v3_0
{
	public class AuthenticationService
	{
		public readonly SignInManager<ApplicationUser> SignInManager;
		public readonly UserManager<ApplicationUser> UserManager;
		public readonly RoleManager<ApplicationRole> RoleManager;
		private readonly AppTokenSettings _appTokenSettingsSettings;
		private readonly IAContext _context;

		private readonly IJwtService _jwksService;
		private readonly IAspNetUser _aspNetUser;

		public AuthenticationService(
			SignInManager<ApplicationUser> signInManager,
			UserManager<ApplicationUser> userManager,
			IOptions<AppTokenSettings> appTokenSettingsSettings,
			IAContext context,
			IJwtService jwksService,
			IAspNetUser aspNetUser, RoleManager<ApplicationRole> roleManager)
		{
			SignInManager = signInManager;
			UserManager = userManager;
			_appTokenSettingsSettings = appTokenSettingsSettings.Value;
			_jwksService = jwksService;
			_aspNetUser = aspNetUser;
			RoleManager = roleManager;
			_context = context;
		}

		public async Task<UserResponseLogin> CreateJwt(string email)
		{
			var user = await UserManager.FindByEmailAsync(email);
			var claims = await UserManager.GetClaimsAsync(user);

			var identityClaims = await GetClaimsUser(claims, user);
			var encodedToken = EncodeToken(identityClaims);

			var refreshToken = await CreateRefreshToken(email);

			return GetResponseToken(await encodedToken, user, claims, refreshToken);
		}

		private async Task<ClaimsIdentity> GetClaimsUser(ICollection<Claim> claims, ApplicationUser user)
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

		private async Task<string> EncodeToken(ClaimsIdentity identityClaims)
		{
			var tokenHandler = new JwtSecurityTokenHandler();
			var currentIssuer = $"{_aspNetUser.GetHttpContext().Request.Scheme}://{_aspNetUser.GetHttpContext().Request.Host}";
			var key = await _jwksService.GetCurrentSigningCredentials();
			var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
			{
				Issuer = currentIssuer,
				Subject = identityClaims,
				Expires = DateTime.UtcNow.AddMinutes(5),
				SigningCredentials = key,
			});

			return tokenHandler.WriteToken(token);
		}

		private UserResponseLogin GetResponseToken(string encodedToken, ApplicationUser user, IEnumerable<Claim> claims, RefreshToken refreshToken)
		{
			return new()
			{
				AccessToken = encodedToken,
				RefreshToken = refreshToken.Token,
				ExpiresIn = TimeSpan.FromMinutes(5).TotalSeconds,
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

		public async Task<RefreshToken> GetRefreshToken(Guid tokenG)
		{
			var token = await _context.RefreshTokens.AsNoTracking().FirstOrDefaultAsync(u => u.Token == tokenG);

			return token != null && token.ExpirationDate.ToLocalTime() > DateTime.Now ? token : null;
		}

		public async Task<RefreshToken> GetRefreshToken(string email)
		{
			var token = await _context.RefreshTokens.AsNoTracking().FirstOrDefaultAsync(u => u.UserEmail == email);

			return token != null && token.ExpirationDate.ToLocalTime() > DateTime.Now ? token : null;
		}
	}
}