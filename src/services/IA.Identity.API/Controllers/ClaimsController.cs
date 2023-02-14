using IA.Identity.API.Identity;
using IA.Identity.API.Models;
using IA.Identity.API.Services.v3_0;
using IA.WebAPI.Core.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IA.Identity.API.Controllers
{
	[ApiVersion("1.0", Deprecated = true)]
	[ApiVersion("2.0")]
	[ApiVersion("3.0")]
	[Route("api/v{version:apiVersion}/[controller]")]
	public class ClaimsController : MainController
	{
		private readonly AuthenticationService _authenticationService;

		public ClaimsController(AuthenticationService authenticationService)
		{
			_authenticationService = authenticationService;
		}

		[HttpGet]
		[Route("claims")]
		[AllowAnonymous]
		[ProducesResponseType(typeof(IList<Claim>), (int)HttpStatusCode.OK)]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> GetAllClaims()
		{
			var users = await _authenticationService.UserManager.Users.ToListAsync();

			IList<Claim> claims = null;
			foreach (var user in users)
			{
				var claimsUser = await _authenticationService.UserManager.GetClaimsAsync(user);

				if (claimsUser == null) continue;

				claims ??= new List<Claim>();

				foreach (var claimUser in claimsUser)
				{
					claims.Add(new Claim(claimUser.Type, claimUser.Value));
				}
			}

			if (claims == null) return NotFound();

			var result = claims
				.GroupBy(c => new { c.Type }, (key, g) => new { key, g })
				.Select(g => new { g.key.Type }).ToList();

			return Ok(result);
		}

		[HttpGet]
		[Route("claim/{type}/users")]
		[AllowAnonymous]
		[ProducesResponseType(typeof(IList<ClaimUserViewModel>), (int)HttpStatusCode.OK)]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> GetUsersByClaim([FromRoute] string type)
		{
			var users = await _authenticationService.UserManager.Users.ToListAsync();

			IList<ClaimUserViewModel> claimUsers = null;
			foreach (var user in users)
			{
				var claimsUser = await _authenticationService.UserManager.GetClaimsAsync(user);
				if (claimsUser == null) continue;

				var claim = claimsUser.SingleOrDefault(x => x.Type.Equals(type));
				if (claim == null) continue;

				if (claimUsers == null)
					claimUsers = new List<ClaimUserViewModel>();
				claimUsers.Add(
					new ClaimUserViewModel
					{
						Type = claim.Type,
						Value = claim.Value,
						UserResponse = new UserResponse
						{
							Id = user.Id,
							FirstName = user.FirstName,
							LastName = user.LastName,
							ITIN = user.ITIN,
							Email = user.Email
						}
					});
			}

			if (claimUsers == null) return NotFound();

			return CustomResponse(claimUsers);
		}

		[HttpPost]
		[Route("claim")]
		[AllowAnonymous]
		[ProducesResponseType((int)HttpStatusCode.NoContent)]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> Post(ClaimViewModel claimViewModel)
		{
			var user = await _authenticationService.UserManager.Users.FirstOrDefaultAsync(u => u.Id.Equals(claimViewModel.UserId));

			if (user == null) return NotFound();

			var claimsUser = await _authenticationService.UserManager.GetClaimsAsync(user);
			if (claimsUser.Any(c => c.Type == claimViewModel.Type))
				await _authenticationService.UserManager.RemoveClaimAsync(user, claimsUser.SingleOrDefault(c => c.Type == claimViewModel.Type));

			var claim = new Claim(claimViewModel.Type, claimViewModel.Value);
			await _authenticationService.UserManager.AddClaimAsync(user, claim);

			return CustomResponse(claim);
		}

		[HttpPost]
		[Route("CurrentUser")]
		[AllowAnonymous]
		[ProducesResponseType((int)HttpStatusCode.NoContent)]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> CurrentUser(ClaimCurrentUserViewModel claimViewModel)
		{
			var user = await _authenticationService.UserManager.GetUserAsync(HttpContext.User);

			if (user == null) return NotFound();

			var claimsUser = await _authenticationService.UserManager.GetClaimsAsync(user);
			if (claimsUser.Any(c => c.Type == claimViewModel.Type))
				await _authenticationService.UserManager.RemoveClaimAsync(user, claimsUser.SingleOrDefault(c => c.Type == claimViewModel.Type));

			await _authenticationService.UserManager.AddClaimAsync(user, new Claim(claimViewModel.Type, claimViewModel.Value));

			return CustomResponse();
		}

		[HttpPut]
		[Route("claimType")]
		[AllowAnonymous]
		[ProducesResponseType((int)HttpStatusCode.NoContent)]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> PutClaimType(ClaimUpdateTypeViewMode claimUpdateViewMode)
		{
			var users = await _authenticationService.UserManager.Users.ToListAsync();

			foreach (var user in users)
			{
				var claimsUser = await _authenticationService.UserManager.GetClaimsAsync(user);
				if (claimsUser == null) continue;

				var oldClaim = claimsUser.SingleOrDefault(x => x.Type.Equals(claimUpdateViewMode.OldType));
				if (oldClaim == null) continue;

				await _authenticationService.UserManager.RemoveClaimAsync(user, claimsUser.SingleOrDefault(c => c.Type == oldClaim.Type));
				await _authenticationService.UserManager.AddClaimAsync(user, new Claim(claimUpdateViewMode.NewType, oldClaim.Value));
			}

			return CustomResponse();
		}

		[HttpPut]
		[Route("claim/{type}/user/{user:int}")]
		[AllowAnonymous]
		[ProducesResponseType((int)HttpStatusCode.NoContent)]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> PutClaimValue(string type, int user, ClaimUpdateValueViewMode claimUpdateViewMode)
		{
			var userClaim = await _authenticationService.UserManager.Users.FirstOrDefaultAsync(u => u.Id.Equals(user));

			var claimsUser = await _authenticationService.UserManager.GetClaimsAsync(userClaim);
			if (claimsUser == null) return NotFound();

			var claim = claimsUser.SingleOrDefault(x => x.Type.Equals(type));
			if (claim == null) return NotFound();

			var newClaim = new Claim(type, claimUpdateViewMode.Value);

			await _authenticationService.UserManager.RemoveClaimAsync(userClaim, claimsUser.SingleOrDefault(c => c.Type == type));
			await _authenticationService.UserManager.AddClaimAsync(userClaim, newClaim);

			return CustomResponse(newClaim);
		}

		[HttpDelete]
		[Route("CurrentUser/{type}")]
		[AllowAnonymous]
		[ProducesResponseType((int)HttpStatusCode.NoContent)]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> CurrentUser([FromRoute] string type)
		{
			var user = await _authenticationService.UserManager.GetUserAsync(HttpContext.User);

			if (user == null) return NotFound();

			var claimsUser = await _authenticationService.UserManager.GetClaimsAsync(user);
			if (claimsUser.Any(c => c.Type == type))
				await _authenticationService.UserManager.RemoveClaimAsync(user, claimsUser.SingleOrDefault(c => c.Type == type));

			return CustomResponse();
		}

		[HttpDelete]
		[Route("claim/{type}")]
		[AllowAnonymous]
		[ProducesResponseType((int)HttpStatusCode.NoContent)]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> Delete([FromRoute] string type)
		{
			var users = await _authenticationService.UserManager.Users.ToListAsync();

			foreach (var user in users)
			{
				var claimsUser = await _authenticationService.UserManager.GetClaimsAsync(user);
				if (claimsUser == null) continue;

				var claim = claimsUser.SingleOrDefault(x => x.Type.Equals(type));
				if (claim == null) continue;

				await _authenticationService.UserManager.RemoveClaimAsync(user, claimsUser.SingleOrDefault(c => c.Type == claim.Type));
			}

			return CustomResponse();
		}

		[HttpDelete]
		[Route("claim/{type}/user/{user:int}")]
		[AllowAnonymous]
		[ProducesResponseType((int)HttpStatusCode.NoContent)]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> DeleteClaimUser([FromRoute] string type, [FromRoute] int user)
		{
			var userClaim = await _authenticationService.UserManager.Users.FirstOrDefaultAsync(u => u.Id.Equals(user));

			var claimsUser = await _authenticationService.UserManager.GetClaimsAsync(userClaim);
			if (claimsUser == null) return NotFound();

			var claim = claimsUser.SingleOrDefault(x => x.Type.Equals(type));
			if (claim == null) return NotFound();

			await _authenticationService.UserManager.RemoveClaimAsync(userClaim, claimsUser.SingleOrDefault(c => c.Type == claim.Type));

			return CustomResponse();
		}
	}
}
