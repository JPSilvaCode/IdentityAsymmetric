using IA.Identity.API.Identity;
using IA.Identity.API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NetDevPack.Security.Jwt.Model;
using NetDevPack.Security.Jwt.Store.EntityFrameworkCore;

namespace IA.Identity.API.Data
{
    public class IAContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>, ISecurityKeyContext
    {
        public IAContext(DbContextOptions<IAContext> options) : base(options) { }

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public DbSet<SecurityKeyWithPrivate> SecurityKeys { get; set; }
    }
}