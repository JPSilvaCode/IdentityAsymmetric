using IA.Identity.API.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IA.Identity.API.Data
{
    public class IAContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
    {
        public IAContext(DbContextOptions<IAContext> options) : base(options) { }
    }
}