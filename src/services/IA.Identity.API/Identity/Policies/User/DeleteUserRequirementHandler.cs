using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace IA.Identity.API.Identity.Policies.User
{
    public class DeleteUserRequirementHandler : AuthorizationHandler<DeleteUserRequirement>
    {
        private const string AdministratorRoleName = "Admin";

        private AuthorizationHandlerContext _context;

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, DeleteUserRequirement requirement)
        {
            _context = context;

            var isAdministrator = IsAdministrator();
            var canDeleteUser = HasRequirements(requirement);

            if (isAdministrator && canDeleteUser) context.Succeed(requirement);

            return Task.CompletedTask;
        }

        private bool IsAdministrator()
            => GetClaim(ClaimTypes.Role, AdministratorRoleName);

        private bool HasRequirements(DeleteUserRequirement requirement)
            => GetClaim("User", requirement.RequiredPermission);

        private bool GetClaim(string type, string value)
            => _context.User.Claims.Any(c => c.Type == type && c.Value.Split(',').Contains(value));
    }
}