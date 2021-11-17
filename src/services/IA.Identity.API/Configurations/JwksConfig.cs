using IA.Identity.API.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using NetDevPack.Security.Jwt;
using NetDevPack.Security.Jwt.AspNetCore;
using NetDevPack.Security.Jwt.Store.EntityFrameworkCore;
using System;

namespace IA.Identity.API.Configurations
{
    public static class JwksConfig
    {
        public static void AddJwksConfiguration(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddJwksManager(options => options.Jws = JwsAlgorithm.ES256).PersistKeysToDatabaseStore<IAContext>();
        }

        public static IApplicationBuilder UseJwksConfiguration(this IApplicationBuilder app)
        {
            app.UseJwksDiscovery();

            return app;
        }
    }
}
