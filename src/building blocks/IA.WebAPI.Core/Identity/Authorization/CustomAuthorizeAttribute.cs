using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IA.WebAPI.Core.Identity.Authorization
{
    public class CustomAuthorizeAttribute : TypeFilterAttribute
    {
        public CustomAuthorizeAttribute(string claimName, string claimValue) : base(typeof(RequerimentClaimFilter))
            => Arguments = new[] { (object)new Claim(claimName, claimValue) };
    }
}