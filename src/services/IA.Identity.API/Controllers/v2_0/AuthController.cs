﻿using System;
using IA.Identity.API.Models;
using IA.Identity.API.Services.v2_0;
using IA.WebAPI.Core.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IA.Identity.API.Controllers.v2_0
{
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AuthController : MainController
    {
        private readonly AuthenticationService _authenticationService;

        public AuthController(AuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        [HttpPost, MapToApiVersion("2.0")]
        [Route("login")]
        [AllowAnonymous]
        public async Task<ActionResult> Login(UserLogin userLogin)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);
            var result = await _authenticationService.SignInManager.PasswordSignInAsync(userLogin.Email, userLogin.Password, false, true);

            if (result.Succeeded)
            {
                return CustomResponse(await _authenticationService.CreateJwt(userLogin.Email));
            }

            if (result.IsLockedOut)
            {
                AddProcessingError("Usuário temporariamente bloqueado por tentativas inválidas");
                return CustomResponse();
            }

            if (!await _authenticationService.UserManager.IsEmailConfirmedAsync(await _authenticationService.UserManager.FindByEmailAsync(userLogin.Email)))
            {
                AddProcessingError("O e-mail não foi confirmado, confirme primeiro");
                return CustomResponse();
            }

            AddProcessingError("Usuário ou Senha incorretos");
            return CustomResponse();
        }

        [HttpPost, MapToApiVersion("2.0")]
        [Route("refresh-token")]
        public async Task<IActionResult> RefreshToken(string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                AddProcessingError("Refresh Token inválido");
                return CustomResponse();
            }

            var token = await _authenticationService.GetRefreshToken(Guid.Parse(refreshToken));

            if (token is not null) return CustomResponse(await _authenticationService.CreateJwt(token.UserEmail));

            AddProcessingError("Refresh Token expirado");
            return CustomResponse();
        }
    }
}
