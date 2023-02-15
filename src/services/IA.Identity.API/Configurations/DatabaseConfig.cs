using IA.Identity.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace IA.Identity.API.Configurations
{
	public static class DatabaseConfig
	{
		public static void AddDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration)
		{
			if (services == null) throw new ArgumentNullException(nameof(services));

			services.AddDbContext<IAContext>(options =>
				options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")), ServiceLifetime.Transient);
		}
	}
}