using System;
using System.Net;
using IA.Identity.API.Models;
using IA.Identity.API.Services.v3_0;
using IA.WebAPI.Core.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IA.Identity.API.Controllers.v3_0
{
	[ApiVersion("3.0")]
	[Route("api/v{version:apiVersion}/[controller]")]
	public class AuthController : MainController
	{
		private readonly AuthenticationService _authenticationService;

		public AuthController(AuthenticationService authenticationService)
		{
			_authenticationService = authenticationService;
		}

		[HttpPost, MapToApiVersion("3.0")]
		[Route("login")]
		[AllowAnonymous]
		[ProducesResponseType(typeof(UserResponseLogin), (int)HttpStatusCode.OK)]
		[ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
		public async Task<ActionResult> Login(UserLogin userLogin)
		{
			if (!ModelState.IsValid) return CustomResponse(ModelState);
			var result = await _authenticationService.SignInManager.PasswordSignInAsync(userLogin.Email, userLogin.Password, false, true);

			if (result.Succeeded)
			{
				if (!await _authenticationService.UserManager.IsEmailConfirmedAsync(await _authenticationService.UserManager.FindByEmailAsync(userLogin.Email)))
				{
					AddProcessingError("O e-mail não foi confirmado, confirme primeiro");
					return CustomResponse();
				}

				return CustomResponse(await _authenticationService.CreateJwt(userLogin.Email));
			}

			if (result.IsLockedOut)
			{
				AddProcessingError("Usuário temporariamente bloqueado por tentativas inválidas");
				return CustomResponse();
			}

			AddProcessingError("Usuário ou Senha incorretos");
			return CustomResponse();
		}

		[HttpPost, MapToApiVersion("3.0")]
		[Route("refresh-token-guid/{refreshToken}")]
		[ProducesResponseType(typeof(UserResponseLogin), (int)HttpStatusCode.OK)]
		[ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
		public async Task<IActionResult> RefreshTokenGuid([FromRoute] string refreshToken)
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

		[HttpPost, MapToApiVersion("3.0")]
		[Route("refresh-token-email/{email}")]
		[ProducesResponseType(typeof(UserResponseLogin), (int)HttpStatusCode.OK)]
		[ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
		public async Task<IActionResult> RefreshTokenEmail([FromRoute] string email)
		{
			if (string.IsNullOrEmpty(email))
			{
				AddProcessingError("Refresh Email inválido");
				return CustomResponse();
			}

			var token = await _authenticationService.GetRefreshToken(email);

			if (token is not null) return CustomResponse(await _authenticationService.CreateJwt(token.UserEmail));

			AddProcessingError("Refresh Token expirado");
			return CustomResponse();
		}
	}
}
