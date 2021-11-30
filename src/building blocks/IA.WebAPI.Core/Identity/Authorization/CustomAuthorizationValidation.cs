using Microsoft.AspNetCore.Http;
using System.Linq;

namespace IA.WebAPI.Core.Identity.Authorization
{
    public static class CustomAuthorizationValidation
    {
        public static bool UserHasValidClaim(HttpContext context, string claimName, string claimValue)
            => context.User.Identity is {IsAuthenticated: true} && context.User.Claims.Any(c => c.Type == claimName && c.Value.Split(',').Contains(claimValue));
    }
}