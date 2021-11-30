using Microsoft.AspNetCore.Authorization;

namespace IA.Identity.API.Identity.Policies.User
{
    public class DeleteUserRequirement : IAuthorizationRequirement
    {
        public string RequiredPermission { get; }

        public DeleteUserRequirement(string requiredPermission)
        {
            RequiredPermission = requiredPermission;
        }
    }
}