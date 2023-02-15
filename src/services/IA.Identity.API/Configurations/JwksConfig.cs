using IA.Identity.API.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace IA.Identity.API.Configurations
{
    public static class JwksConfig
    {
        public static void AddJwksConfiguration(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

			services.AddJwksManager().PersistKeysToDatabaseStore<IAContext>();
		}

        public static IApplicationBuilder UseJwksConfiguration(this IApplicationBuilder app)
        {
            app.UseJwksDiscovery();

            return app;
        }
    }
}
