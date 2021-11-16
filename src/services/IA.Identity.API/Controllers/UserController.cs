using IA.Identity.API.Identity;
using IA.Identity.API.Models;
using IA.Identity.API.Services;
using IA.WebAPI.Core.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace IA.Identity.API.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class UserController : MainController
    {
        private readonly AuthenticationService _authenticationService;
        private readonly IEmailService _emailSender;

        public UserController(AuthenticationService authenticationService, IEmailService emailSender)
        {
            _authenticationService = authenticationService;
            _emailSender = emailSender;
        }

        [HttpPost, MapToApiVersion("1.0")]
        [Route("register")]
        [AllowAnonymous]
        public async Task<ActionResult> Register(UserRegister userRegister)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var user = new ApplicationUser
            {
                FirstName = userRegister.FirstName,
                LastName = userRegister.LastName,
                ITIN = userRegister.ITIN,
                UserName = userRegister.Email,
                Email = userRegister.Email
            };

            var result = await _authenticationService.UserManager.CreateAsync(user, userRegister.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    AddProcessingError(error.Description);

                return CustomResponse();
            }

            var token = await _authenticationService.UserManager.GenerateEmailConfirmationTokenAsync(user);

            await _emailSender.SendEmailAsync(user.Email,
                "Confirme sua conta",
                "Por favor, confirme sua conta clicando aqui <a href=\"" + Url.Action("ConfirmEmail", "User", new { userId = user.Id, token }, Request.Scheme) + "\">here</a>");

            var userResponse = new UserResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                ITIN = user.ITIN,
                Email = user.Email
            };

            return Created(Url.Action("GetUserById", "User", new { id = user.Id }, Request.Scheme), userResponse);
        }

        [HttpGet, MapToApiVersion("1.0")]
        [Route("ConfirmEmail")]
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(int userId = 0, string token = "")
        {
            if (userId.Equals(0) || string.IsNullOrWhiteSpace(token))
            {
                AddProcessingError("ID de usuário e código são obrigatórios");
                return CustomResponse();
            }

            var user = await _authenticationService.UserManager.Users.FirstOrDefaultAsync(u => u.Id.Equals(userId));

            if (user == null) return NotFound();

            var result = await _authenticationService.UserManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                var userResponse = new UserResponse
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    ITIN = user.ITIN,
                    Email = user.Email
                };

                return Ok(userResponse);
            }

            foreach (var error in result.Errors)
            {
                AddProcessingError(error.Description);
            }

            return CustomResponse();
        }

        [HttpGet, MapToApiVersion("1.0")]
        [Route("user/{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _authenticationService.UserManager.Users.FirstOrDefaultAsync(u => u.Id.Equals(id));

            if (user == null) return NotFound();

            var userResponse = new UserResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                ITIN = user.ITIN,
                Email = user.Email
            };

            return Ok(userResponse);
        }
    }
}
