using IA.Identity.API.Identity;
using IA.Identity.API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IA.Identity.API.Data
{
    public class IAContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
    {
        public IAContext(DbContextOptions<IAContext> options) : base(options) { }

        public DbSet<RefreshToken> RefreshTokens { get; set; }
    }
}