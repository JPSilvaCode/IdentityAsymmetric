using IA.Identity.API.Models;
using IA.Identity.API.Services.v3_0;
using IA.WebAPI.Core.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using IA.Identity.API.Identity;
using Microsoft.AspNetCore.Identity;
using System.Data;
using System.Linq;

namespace IA.Identity.API.Controllers
{
	[ApiVersion("1.0", Deprecated = true)]
	[ApiVersion("2.0")]
	[ApiVersion("3.0")]
	[Route("api/v{version:apiVersion}/[controller]")]
	public class RolesController : MainController
	{
		private readonly AuthenticationService _authenticationService;

		public RolesController(AuthenticationService authenticationService)
		{
			_authenticationService = authenticationService;
		}

		[HttpGet]
		[Route("{id:int}")]
		[AllowAnonymous]
		[ProducesResponseType(typeof(IList<ApplicationRole>), (int)HttpStatusCode.OK)]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> GetRoleById(int id)
		{
			var role = await _authenticationService.RoleManager.FindByIdAsync(id.ToString());

			return role == null ? NotFound() : CustomResponse(role);
		}

		[HttpGet]
		[Route("{name}")]
		[AllowAnonymous]
		[ProducesResponseType(typeof(IList<ApplicationRole>), (int)HttpStatusCode.OK)]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> GetRoleById(string name)
		{
			var role = await _authenticationService.RoleManager.FindByNameAsync(name);

			return role == null ? NotFound() : CustomResponse(role);
		}

		[HttpGet]
		[Route("roles")]
		[AllowAnonymous]
		[ProducesResponseType(typeof(IList<ApplicationRole>), (int)HttpStatusCode.OK)]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> GetAllRoles()
		{
			var roles = await _authenticationService.RoleManager.Roles.ToListAsync();

			return roles == null ? NotFound() : CustomResponse(roles);
		}

		[HttpPost]
		[Route("role/{name}")]
		[AllowAnonymous]
		[ProducesResponseType(typeof(IList<ApplicationRole>), (int)HttpStatusCode.Created)]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		public async Task<IActionResult> Post([FromRoute] string name)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

			var result = await _authenticationService.RoleManager.CreateAsync(new ApplicationRole { Name = name });

			if (!result.Succeeded)
			{
				foreach (var error in result.Errors)
					AddProcessingError(error.Description);

				return CustomResponse();
			}

			var role = await _authenticationService.RoleManager.FindByNameAsync(name);

			return Created(Url.Action("GetRoleById", "Roles", new { id = role.Id }, Request.Scheme), role);
		}

		[HttpPut]
		[Route("role")]
		[AllowAnonymous]
		[ProducesResponseType(typeof(IList<ApplicationRole>), (int)HttpStatusCode.Created)]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		public async Task<IActionResult> Put(RoleChange roleChange)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

			var role = await _authenticationService.RoleManager.FindByNameAsync(roleChange.OldName);

			if (role == null) return NotFound();

			role.Name = roleChange.NewName;

			var result = await _authenticationService.RoleManager.UpdateAsync(role);

			if (result.Succeeded) return Created(Url.Action("GetRoleById", "Roles", new { id = role.Id }, Request.Scheme), role);

			foreach (var error in result.Errors)
				AddProcessingError(error.Description);

			return CustomResponse();

		}

		[HttpDelete]
		[Route("{id:int}")]
		[AllowAnonymous]
		[ProducesResponseType((int)HttpStatusCode.NoContent)]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> DeleteById(int id)
		{
			var role = await _authenticationService.RoleManager.FindByIdAsync(id.ToString());

			if (role == null) return NotFound();

			var result = await _authenticationService.RoleManager.DeleteAsync(role);

			if (result.Succeeded) return CustomResponse();

			foreach (var error in result.Errors)
				AddProcessingError(error.Description);

			return CustomResponse();
		}

		[HttpDelete("{name}")]
		[AllowAnonymous]
		[ProducesResponseType((int)HttpStatusCode.NoContent)]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> DeleteByName(string name)
		{
			var role = await _authenticationService.RoleManager.FindByNameAsync(name);

			if (role == null) return NotFound();

			var result = await _authenticationService.RoleManager.DeleteAsync(role);

			if (result.Succeeded) return CustomResponse();

			foreach (var error in result.Errors)
				AddProcessingError(error.Description);

			return CustomResponse();
		}

		[HttpGet]
		[AllowAnonymous]
		[Route("Role/{id:int}/Users")]
		[ProducesResponseType((int)HttpStatusCode.NoContent)]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> GetUsersInRole([FromRoute] int id)
		{
			var role = await _authenticationService.RoleManager.FindByIdAsync(id.ToString());

			if (role == null) return NotFound();

			var users = await _authenticationService.UserManager.Users.ToListAsync();
			IList<UserResponse> usersResponse = null;
			foreach (var user in users)
			{
				if (await _authenticationService.UserManager.IsInRoleAsync(user, role.Name))
				{
					if (usersResponse == null)
						usersResponse = new List<UserResponse>();

					usersResponse.Add(
						new UserResponse
						{
							Id = user.Id,
							FirstName = user.FirstName,
							LastName = user.LastName,
							ITIN = user.ITIN,
							Email = user.Email
						}
					);
				}
			}

			var roleVM = new RoleUsersVIewModel
			{
				Id = id,
				Name = role.Name,
				UsersResponse = usersResponse
			};

			return CustomResponse(roleVM);
		}

		[HttpPost]
		[AllowAnonymous]
		[Route("Role/{id:int}/User/{userId:int}")]
		[ProducesResponseType((int)HttpStatusCode.NoContent)]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> AddUserInRole([FromRoute] int id, [FromRoute] int userId)
		{
			var role = await _authenticationService.RoleManager.FindByIdAsync(id.ToString());

			if (role == null) return NotFound();

			var user = await _authenticationService.UserManager.FindByIdAsync(userId.ToString());

			if (user == null)
			{
				AddProcessingError($"Usuário: {userId} não existe.");
				return CustomResponse();
			}

			if (await _authenticationService.UserManager.IsInRoleAsync(user, role.Name))
			{
				AddProcessingError($"Usuário: {userId} já possui a role {role.Name}.");
				return CustomResponse();
			}

			var result = await _authenticationService.UserManager.AddToRoleAsync(user, role.Name);

			if (!result.Succeeded) AddProcessingError($"User: {userId} could not be added to role");

			return CustomResponse();
		}

		[HttpPost]
		[AllowAnonymous]
		[Route("Role/{id:int}/UsersById")]
		[ProducesResponseType((int)HttpStatusCode.NoContent)]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> AddUsersInRole([FromRoute] int id, [FromBody] List<int> usersId)
		{
			var role = await _authenticationService.RoleManager.FindByIdAsync(id.ToString());

			if (role == null) return NotFound();

			foreach (var userId in usersId)
			{
				var user = await _authenticationService.UserManager.FindByIdAsync(userId.ToString());

				if (user == null)
				{
					AddProcessingError($"Usuário: {userId} não existe.");
					continue;
				}

				if (await _authenticationService.UserManager.IsInRoleAsync(user, role.Name)) continue;

				var result = await _authenticationService.UserManager.AddToRoleAsync(user, role.Name);

				if (!result.Succeeded) AddProcessingError($"User: {userId} could not be added to role");
			}

			return CustomResponse();
		}

		[HttpDelete]
		[AllowAnonymous]
		[Route("Role/{id:int}/User/{userId:int}")]
		[ProducesResponseType((int)HttpStatusCode.NoContent)]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> RemovedUsersInRole([FromRoute] int id, [FromRoute] int userId)
		{
			var role = await _authenticationService.RoleManager.FindByIdAsync(id.ToString());

			if (role == null) return NotFound();

			var user = await _authenticationService.UserManager.FindByIdAsync(userId.ToString());

			if (user == null)
			{
				AddProcessingError($"Usuário: {userId} não existe.");
				return CustomResponse();
			}

			var result = await _authenticationService.UserManager.RemoveFromRoleAsync(user, role.Name);

			if (!result.Succeeded) AddProcessingError($"User: {userId} could not be removed from role");

			return CustomResponse();
		}

		[HttpDelete]
		[AllowAnonymous]
		[Route("RoleById/{id:int}/UsersById")]
		[ProducesResponseType((int)HttpStatusCode.NoContent)]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> RemovedUsersInRole([FromRoute] int id, [FromBody] List<int> usersId)
		{
			var role = await _authenticationService.RoleManager.FindByIdAsync(id.ToString());

			if (role == null) return NotFound();

			foreach (var userId in usersId)
			{
				var user = await _authenticationService.UserManager.FindByIdAsync(userId.ToString());

				if (user == null)
				{
					AddProcessingError($"Usuário: {userId} não existe.");
					continue;
				}

				var result = await _authenticationService.UserManager.RemoveFromRoleAsync(user, role.Name);

				if (!result.Succeeded) AddProcessingError($"User: {userId} could not be removed from role");
			}

			return CustomResponse();
		}

		[HttpPost]
		[AllowAnonymous]
		[Route("RoleById/{id:int}/UsersByEmail")]
		[ProducesResponseType((int)HttpStatusCode.NoContent)]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> AddUsersInRole([FromRoute] int id, [FromBody] List<string> usersEmail)
		{
			var role = await _authenticationService.RoleManager.FindByIdAsync(id.ToString());

			if (role == null) return NotFound();

			foreach (var userEmail in usersEmail)
			{
				var user = await _authenticationService.UserManager.FindByIdAsync(userEmail);

				if (user == null)
				{
					AddProcessingError($"Usuário: {userEmail} não existe.");
					continue;
				}

				if (await _authenticationService.UserManager.IsInRoleAsync(user, role.Name)) continue;

				var result = await _authenticationService.UserManager.AddToRoleAsync(user, role.Name);

				if (!result.Succeeded) AddProcessingError($"User: {userEmail} could not be added to role");
			}

			return CustomResponse();
		}

		[HttpDelete]
		[AllowAnonymous]
		[Route("RoleById/{id:int}/UsersByEmail")]
		[ProducesResponseType((int)HttpStatusCode.NoContent)]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> RemovedUsersInRole([FromRoute] int id, [FromBody] List<string> usersEmail)
		{
			var role = await _authenticationService.RoleManager.FindByIdAsync(id.ToString());

			if (role == null) return NotFound();

			foreach (var userEmail in usersEmail)
			{
				var user = await _authenticationService.UserManager.FindByIdAsync(userEmail);

				if (user == null)
				{
					AddProcessingError($"Usuário: {userEmail} não existe.");
					continue;
				}

				var result = await _authenticationService.UserManager.RemoveFromRoleAsync(user, role.Name);

				if (!result.Succeeded) AddProcessingError($"User: {userEmail} could not be removed from role");
			}

			return CustomResponse();
		}

		[HttpPost]
		[AllowAnonymous]
		[Route("RoleByName/{name}/UsersById")]
		[ProducesResponseType((int)HttpStatusCode.NoContent)]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> AddUsersInRole([FromRoute] string name, [FromBody] List<int> usersId)
		{
			var role = await _authenticationService.RoleManager.FindByNameAsync(name);

			if (role == null) return NotFound();

			foreach (var userId in usersId)
			{
				var user = await _authenticationService.UserManager.FindByIdAsync(userId.ToString());

				if (user == null)
				{
					AddProcessingError($"Usuário: {userId} não existe.");
					continue;
				}

				if (await _authenticationService.UserManager.IsInRoleAsync(user, role.Name)) continue;

				var result = await _authenticationService.UserManager.AddToRoleAsync(user, role.Name);

				if (!result.Succeeded) AddProcessingError($"User: {userId} could not be added to role");
			}

			return CustomResponse();
		}

		[HttpDelete]
		[AllowAnonymous]
		[Route("RoleByName/{name}/UsersById")]
		[ProducesResponseType((int)HttpStatusCode.NoContent)]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> RemovedUsersInRole([FromRoute] string name, [FromBody] List<int> usersId)
		{
			var role = await _authenticationService.RoleManager.FindByNameAsync(name);

			if (role == null) return NotFound();

			foreach (var userId in usersId)
			{
				var user = await _authenticationService.UserManager.FindByIdAsync(userId.ToString());

				if (user == null)
				{
					AddProcessingError($"Usuário: {userId} não existe.");
					continue;
				}

				var result = await _authenticationService.UserManager.RemoveFromRoleAsync(user, role.Name);

				if (!result.Succeeded) AddProcessingError($"User: {userId} could not be removed from role");
			}

			return CustomResponse();
		}

		[HttpPost]
		[AllowAnonymous]
		[Route("RoleByName/{name}/UsersByEmail")]
		[ProducesResponseType((int)HttpStatusCode.NoContent)]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> AddUsersInRole([FromRoute] string name, [FromBody] List<string> usersEmail)
		{
			var role = await _authenticationService.RoleManager.FindByNameAsync(name);

			if (role == null) return NotFound();

			foreach (var userEmail in usersEmail)
			{
				var user = await _authenticationService.UserManager.FindByIdAsync(userEmail.ToString());

				if (user == null)
				{
					AddProcessingError($"Usuário: {userEmail} não existe.");
					continue;
				}

				if (await _authenticationService.UserManager.IsInRoleAsync(user, role.Name)) continue;

				var result = await _authenticationService.UserManager.AddToRoleAsync(user, role.Name);

				if (!result.Succeeded) AddProcessingError($"User: {userEmail} could not be added to role");
			}

			return CustomResponse();
		}

		[HttpDelete]
		[AllowAnonymous]
		[Route("RoleByName/{name}/UsersByEmail")]
		[ProducesResponseType((int)HttpStatusCode.NoContent)]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> RemovedUsersInRole([FromRoute] string name, [FromBody] List<string> usersEmail)
		{
			var role = await _authenticationService.RoleManager.FindByNameAsync(name);

			if (role == null) return NotFound();

			foreach (var userEmail in usersEmail)
			{
				var user = await _authenticationService.UserManager.FindByIdAsync(userEmail.ToString());

				if (user == null)
				{
					AddProcessingError($"Usuário: {userEmail} não existe.");
					continue;
				}

				var result = await _authenticationService.UserManager.RemoveFromRoleAsync(user, role.Name);

				if (!result.Succeeded) AddProcessingError($"User: {userEmail} could not be removed from role");
			}

			return CustomResponse();
		}
	}
}
