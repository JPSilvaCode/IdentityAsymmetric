using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using IA.Identity.API.Identity;
using IA.Identity.API.Models;
using IA.Identity.API.Services;
using IA.Identity.API.Services.v3_0;
using IA.WebAPI.Core.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using ICWebAPI.Models;
using Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Manage.Internal;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace IA.Identity.API.Controllers
{
    [ApiVersion("1.0", Deprecated = true)]
    [ApiVersion("2.0")]
    [ApiVersion("3.0")]
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

        [HttpPost]
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

        [HttpGet]
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

        [HttpGet]
        [Route("users")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IList<UserResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _authenticationService.UserManager.Users.ToListAsync();

            if (users == null) return NotFound();

            var usersReturn = (from user in users
                               select new
                               {
                                   user.Id,
                                   user.FirstName,
                                   user.ITIN,
                                   user.Email
                               }).ToList().Select(p => new UserResponse
                               {
                                   Id = p.Id,
                                   FirstName = p.FirstName,
                                   ITIN = p.ITIN,
                                   Email = p.Email
                               });

            return CustomResponse(usersReturn);
        }

        [HttpGet]
        [Route("user/{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(UserResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
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

        [HttpGet]
        [Route("userbyname/{name}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(UserResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> GetUserByName(string name)
        {
            var user = await _authenticationService.UserManager.Users.FirstOrDefaultAsync(u => u.UserName.Equals(name));

            if (user == null) return NotFound();

            var userReturn = new UserResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                ITIN = user.ITIN,
                Email = user.Email
            };

            return CustomResponse(userReturn);
        }

        [HttpGet]
        [Route("userbyitin/{itin}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(UserResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> GetUserByITIN(string itin)
        {
            var user = await _authenticationService.UserManager.Users.FirstOrDefaultAsync(u => u.ITIN.Equals(itin));

            if (user == null) return NotFound();

            var userReturn = new UserResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                ITIN = user.ITIN,
                Email = user.Email
            };

            return CustomResponse(userReturn);
        }

        [HttpGet]
        [Route("userbyemail/{email}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(UserResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            var user = await _authenticationService.UserManager.Users.FirstOrDefaultAsync(u => u.Email.Equals(email));

            if (user == null) return NotFound();

            var userReturn = new UserResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                ITIN = user.ITIN,
                Email = user.Email
            };

            return CustomResponse(userReturn);
        }

        [HttpPost]
        [Route("ChangePassword")]
        [AllowAnonymous]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ChangePassword(ChangePassword model)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var user = await _authenticationService.UserManager.Users.FirstOrDefaultAsync(u => u.Email.Equals(model.Email));
            if (user == null) return NotFound();

            if (await _authenticationService.SignInManager.CheckPasswordSignInAsync(user, model.OldPassword, true) != SignInResult.Success)
            {
                AddProcessingError("Usuário ou Senha incorretos");
                return CustomResponse();
            }

            var result = await _authenticationService.UserManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

            if (result.Succeeded) return CustomResponse();

            foreach (var error in result.Errors)
                AddProcessingError(error.Description);

            return CustomResponse();
        }

        [HttpDelete]
        [Route("userbyid/{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _authenticationService.UserManager.FindByIdAsync(id.ToString());

            if (user == null) return NotFound();

            var result = await _authenticationService.UserManager.DeleteAsync(user);

            if (result.Succeeded) return CustomResponse();

            foreach (var error in result.Errors)
                AddProcessingError(error.Description);

            return CustomResponse();
        }

        [HttpDelete]
        [Route("userbyemail/{email}")]
        [AllowAnonymous]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> DeleteUser(string email)
        {
            var user = await _authenticationService.UserManager.FindByEmailAsync(email);

            if (user == null) return NotFound();

            var result = await _authenticationService.UserManager.DeleteAsync(user);

            if (result.Succeeded) return CustomResponse();

            foreach (var error in result.Errors)
                AddProcessingError(error.Description);

            return CustomResponse();
        }

        [HttpGet]
        [Route("user/{id:int}/roles")]
        [AllowAnonymous]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetRolesTouser(int id)
        {
            var user = await _authenticationService.UserManager.FindByIdAsync(id.ToString());

            if (user == null) return NotFound();

            var rolesUser = await _authenticationService.UserManager.GetRolesAsync(user);

            return rolesUser == null ? NotFound() : CustomResponse(rolesUser);
        }

        [HttpGet]
        [Route("user/{email}/roles")]
        [AllowAnonymous]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetRolesTouser(string email)
        {
            var user = await _authenticationService.UserManager.FindByEmailAsync(email);

            if (user == null) return NotFound();

            var rolesUser = await _authenticationService.UserManager.GetRolesAsync(user);

            return rolesUser == null ? NotFound() : CustomResponse(rolesUser);
        }

        [HttpPost]
        [Route("user/{id:int}/role/{role}")]
        [AllowAnonymous]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> AssignRoleToUser(int id, string role)
        {
            var user = await _authenticationService.UserManager.FindByIdAsync(id.ToString());

            if (user == null) return NotFound();

            var roles = await _authenticationService.RoleManager.Roles.ToListAsync();

            if (!roles.Any(r => r.Name.Equals(role)))
            {
                AddProcessingError($"Roles '{role}' não existe no sistema");
                return CustomResponse();
            }

            var currentRolesUser = await _authenticationService.UserManager.GetRolesAsync(user);

            if (currentRolesUser.Any(r => r.Equals(role)))
            {
                AddProcessingError($"Role '{role}' já existe para o usuário");
                return CustomResponse();
            }

            var addResult = await _authenticationService.UserManager.AddToRoleAsync(user, role);

            if (addResult.Succeeded) return Ok();

            foreach (var error in addResult.Errors)
                AddProcessingError(error.Description);

            return CustomResponse();
        }

        [HttpPost]
        [Route("user/{email}/role/{role}")]
        [AllowAnonymous]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> AssignRoleToUser(string email, string role)
        {
            var user = await _authenticationService.UserManager.FindByEmailAsync(email);

            if (user == null) return NotFound();

            var roles = await _authenticationService.RoleManager.Roles.ToListAsync();

            if (!roles.Any(r => r.Name.Equals(role)))
            {
                AddProcessingError($"Roles '{role}' não existe no sistema");
                return CustomResponse();
            }

            var currentRolesUser = await _authenticationService.UserManager.GetRolesAsync(user);

            if (currentRolesUser.Any(r => r.Equals(role)))
            {
                AddProcessingError($"Role '{role}' já existe para o usuário");
                return CustomResponse();
            }

            var addResult = await _authenticationService.UserManager.AddToRoleAsync(user, role);

            if (addResult.Succeeded) return Ok();

            foreach (var error in addResult.Errors)
                AddProcessingError(error.Description);

            return CustomResponse();
        }

        [HttpPost]
        [Route("user/{id:int}/roles")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> AssignRolesToUser([FromRoute] int id, [FromBody] string[] rolesToAssign)
        {
            var user = await _authenticationService.UserManager.FindByIdAsync(id.ToString());

            if (user == null) return NotFound();

            var rolesNotExists = rolesToAssign.Except(_authenticationService.RoleManager.Roles.Select(x => x.Name)).ToArray();

            if (rolesNotExists.Any())
            {
                AddProcessingError($"Roles '{string.Join(",", rolesNotExists)}' não existem no sistema");
                return CustomResponse();
            }

            var currentRolesUser = await _authenticationService.UserManager.GetRolesAsync(user);

            var removeResult = await _authenticationService.UserManager.RemoveFromRolesAsync(user, currentRolesUser.ToArray());

            if (!removeResult.Succeeded)
            {
                foreach (var error in removeResult.Errors)
                    AddProcessingError(error.Description);

                return CustomResponse();
            }

            var addResult = await _authenticationService.UserManager.AddToRolesAsync(user, rolesToAssign);

            if (addResult.Succeeded) return Ok();

            foreach (var error in addResult.Errors)
                AddProcessingError(error.Description);

            return CustomResponse();
        }

        [HttpPost]
        [Route("user/{email}/roles")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> AssignRolesToUser([FromRoute] string email, [FromBody] string[] rolesToAssign)
        {
            var user = await _authenticationService.UserManager.FindByEmailAsync(email);

            if (user == null) return NotFound();

            var rolesNotExists = rolesToAssign.Except(_authenticationService.RoleManager.Roles.Select(x => x.Name)).ToArray();

            if (rolesNotExists.Any())
            {
                AddProcessingError($"Roles '{string.Join(",", rolesNotExists)}' não existem no sistema");
                return CustomResponse();
            }

            var currentRolesUser = await _authenticationService.UserManager.GetRolesAsync(user);

            var removeResult = await _authenticationService.UserManager.RemoveFromRolesAsync(user, currentRolesUser.ToArray());

            if (!removeResult.Succeeded)
            {
                foreach (var error in removeResult.Errors)
                    AddProcessingError(error.Description);

                return CustomResponse();
            }

            var addResult = await _authenticationService.UserManager.AddToRolesAsync(user, rolesToAssign);

            if (addResult.Succeeded) return Ok();

            foreach (var error in addResult.Errors)
                AddProcessingError(error.Description);

            return CustomResponse();
        }

        [HttpDelete]
        [Route("user/{id:int}/role/{role}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> RemoveRoleToUser(int id, string role)
        {
            var user = await _authenticationService.UserManager.FindByIdAsync(id.ToString());

            if (user == null) return NotFound();

            var roles = await _authenticationService.RoleManager.Roles.ToListAsync();

            if (!roles.Any(r => r.Name.Equals(role)))
            {
                AddProcessingError($"Role '{role}' não existe no sistema");
                return CustomResponse();
            }

            var currentRolesUser = await _authenticationService.UserManager.GetRolesAsync(user);

            if (!currentRolesUser.Any(r => r.Equals(role)))
            {
                AddProcessingError($"Role '{role}' não existe para o usuário");
                return CustomResponse();
            }

            var addResult = await _authenticationService.UserManager.RemoveFromRoleAsync(user, role);

            if (addResult.Succeeded) return Ok();

            foreach (var error in addResult.Errors)
                AddProcessingError(error.Description);

            return CustomResponse();
        }

        [HttpDelete]
        [Route("user/{email}/role/{role}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> RemoveRoleToUser(string email, string role)
        {
            var user = await _authenticationService.UserManager.FindByEmailAsync(email);

            if (user == null) return NotFound();

            var roles = await _authenticationService.RoleManager.Roles.ToListAsync();

            if (!roles.Any(r => r.Name.Equals(role)))
            {
                AddProcessingError($"Roles '{role}' não existe no sistema");
                return CustomResponse();
            }

            var currentRolesUser = await _authenticationService.UserManager.GetRolesAsync(user);

            if (!currentRolesUser.Any(r => r.Equals(role)))
            {
                AddProcessingError($"Role '{role}' não existe para o usuário");
                return CustomResponse();
            }

            var addResult = await _authenticationService.UserManager.RemoveFromRoleAsync(user, role);

            if (addResult.Succeeded) return Ok();

            foreach (var error in addResult.Errors)
                AddProcessingError(error.Description);

            return CustomResponse();
        }

        [HttpGet]
        [Route("user/{id:int}/claims")]
        public async Task<IActionResult> GetClaimsTouser(int id)
        {
            var user = await _authenticationService.UserManager.FindByIdAsync(id.ToString());

            if (user == null) return NotFound();

            var claimsUser = await _authenticationService.UserManager.GetClaimsAsync(user);

            if (claimsUser == null) return NotFound();

            IList<Claim> claims = null;
            foreach (var claimUser in claimsUser)
            {
                claims ??= new List<Claim>();
                claims.Add(new Claim(claimUser.Type, claimUser.Value));
            }

            return Ok(claims);
        }

        [HttpGet]
        [Route("user/{email}/claims")]
        [ProducesResponseType(typeof(Claim), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetClaimsTouser(string email)
        {
            var user = await _authenticationService.UserManager.FindByEmailAsync(email);

            if (user == null) return NotFound();

            var claimsUser = await _authenticationService.UserManager.GetClaimsAsync(user);

            if (claimsUser == null) return NotFound();

            IList<Claim> claims = null;
            foreach (var claimUser in claimsUser)
            {
                claims ??= new List<Claim>();
                claims.Add(new Claim(claimUser.Type, claimUser.Value));
            }

            return Ok(claims);
        }

        [HttpPost]
        [Route("user/{id:int}/claim/{type}/{value}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> AssignClaimToUser(int id, string type, string value)
        {
            var user = await _authenticationService.UserManager.FindByIdAsync(id.ToString());

            if (user == null) return NotFound();

            var claimsUser = await _authenticationService.UserManager.GetClaimsAsync(user);
            if (claimsUser.Any(c => c.Type == type))
                await _authenticationService.UserManager.RemoveClaimAsync(user, claimsUser.SingleOrDefault(c => c.Type == type));

            await _authenticationService.UserManager.AddClaimAsync(user, new Claim(type, value));

            return CustomResponse();
        }

        [HttpPost]
        [Route("user/{email}/claim/{type}/{value}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> AssignClaimToUser(string email, string type, string value)
        {
            var user = await _authenticationService.UserManager.FindByEmailAsync(email);

            if (user == null) return NotFound();

            var claimsUser = await _authenticationService.UserManager.GetClaimsAsync(user);
            if (claimsUser.Any(c => c.Type == type))
                await _authenticationService.UserManager.RemoveClaimAsync(user, claimsUser.SingleOrDefault(c => c.Type == type));

            await _authenticationService.UserManager.AddClaimAsync(user, new Claim(type, value));

            return CustomResponse();
        }

        [HttpPost]
        [Route("user/{id:int}/claims")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> AssignClaimsToUser([FromRoute] int id, [FromBody] List<Claim> claims)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _authenticationService.UserManager.FindByIdAsync(id.ToString());

            if (user == null) return NotFound();

            foreach (var claimBinding in claims)
            {
                var claimsUser = await _authenticationService.UserManager.GetClaimsAsync(user);
                if (claimsUser.Any(c => c.Type == claimBinding.Type))
                    await _authenticationService.UserManager.RemoveClaimAsync(user, claimsUser.SingleOrDefault(c => c.Type == claimBinding.Type));

                await _authenticationService.UserManager.AddClaimAsync(user, new Claim(claimBinding.Type, claimBinding.Value));
            }

            return CustomResponse();
        }

        [HttpPost]
        [Route("user/{email}/claims")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> AssignClaimsToUser([FromRoute] string email, [FromBody] List<Claim> claims)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _authenticationService.UserManager.FindByEmailAsync(email);

            if (user == null) return NotFound();

            foreach (var claimBinding in claims)
            {
                var claimsUser = await _authenticationService.UserManager.GetClaimsAsync(user);
                if (claimsUser.Any(c => c.Type == claimBinding.Type))
                    await _authenticationService.UserManager.RemoveClaimAsync(user, claimsUser.SingleOrDefault(c => c.Type == claimBinding.Type));

                await _authenticationService.UserManager.AddClaimAsync(user, new Claim(claimBinding.Type, claimBinding.Value));
            }

            return CustomResponse();
        }

        [HttpDelete]
        [Route("user/{id:int}/claim/{type}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> RemoveClaimFromUser(int id, string type)
        {
            var user = await _authenticationService.UserManager.FindByIdAsync(id.ToString());

            if (user == null) return NotFound();

            var claimsUser = await _authenticationService.UserManager.GetClaimsAsync(user);
            if (claimsUser.Any(c => c.Type == type))
                await _authenticationService.UserManager.RemoveClaimAsync(user, claimsUser.SingleOrDefault(c => c.Type == type));

            return CustomResponse();
        }

        [HttpDelete]
        [Route("user/{email}/claim/{type}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> RemoveClaimFromUser(string email, string type)
        {
            var user = await _authenticationService.UserManager.FindByEmailAsync(email);

            if (user == null) return NotFound();

            var claimsUser = await _authenticationService.UserManager.GetClaimsAsync(user);
            if (claimsUser.Any(c => c.Type == type))
                await _authenticationService.UserManager.RemoveClaimAsync(user, claimsUser.SingleOrDefault(c => c.Type == type));

            return CustomResponse();
        }

        [HttpDelete]
        [Route("user/{id:int}/claims")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> RemoveClaimsFromUser([FromRoute] int id, [FromBody] List<Claim> claims)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _authenticationService.UserManager.FindByIdAsync(id.ToString());

            if (user == null) return NotFound();

            foreach (var claimBinding in claims)
            {
                var claimsUser = await _authenticationService.UserManager.GetClaimsAsync(user);
                if (claimsUser.Any(c => c.Type == claimBinding.Type))
                    await _authenticationService.UserManager.RemoveClaimAsync(user, claimsUser.SingleOrDefault(c => c.Type == claimBinding.Type));
            }

            return NoContent();
        }

        [HttpDelete]
        [Route("user/{email}/claims")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> RemoveClaimsFromUser([FromRoute] string email, [FromBody] List<Claim> claims)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _authenticationService.UserManager.FindByEmailAsync(email);

            if (user == null) return NotFound();

            foreach (var claimBinding in claims)
            {
                var claimsUser = await _authenticationService.UserManager.GetClaimsAsync(user);
                if (claimsUser.Any(c => c.Type == claimBinding.Type))
                    await _authenticationService.UserManager.RemoveClaimAsync(user, claimsUser.SingleOrDefault(c => c.Type == claimBinding.Type));
            }

            return NoContent();
        }
    }
}
