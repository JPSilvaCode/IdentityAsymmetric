using IA.Identity.API.Services.v3_0;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using IA.WebAPI.Core.Controllers;
using Microsoft.AspNetCore.Identity;

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

            var result = claims.GroupBy(c => new { c.Type, c.Value }, (key, g) => new { key, g }).Select(g => new { g.key.Type, g.key.Value }).ToList();

            return Ok(result);
        }

        [HttpPost]
        [Route("claim")]
        [AllowAnonymous]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Post([FromRoute] string type, [FromRoute] string value)
        {
            var user = await _authenticationService.UserManager.GetUserAsync(HttpContext.User);

            if (user == null) return NotFound();

            var claimsUser = await _authenticationService.UserManager.GetClaimsAsync(user);
            if (claimsUser.Any(c => c.Type == type))
                await _authenticationService.UserManager.RemoveClaimAsync(user, claimsUser.SingleOrDefault(c => c.Type == type));

            await _authenticationService.UserManager.AddClaimAsync(user, new Claim(type, value));

            return CustomResponse();
        }


        [HttpDelete]
        [Route("claim/{type}")]
        [AllowAnonymous]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Remove([FromRoute] string type)
        {
            var user = await _authenticationService.UserManager.GetUserAsync(HttpContext.User);

            if (user == null) return NotFound();

            var claimsUser = await _authenticationService.UserManager.GetClaimsAsync(user);
            if (claimsUser.Any(c => c.Type == type))
                await _authenticationService.UserManager.RemoveClaimAsync(user, claimsUser.SingleOrDefault(c => c.Type == type));

            return CustomResponse();
        }
    }
}
