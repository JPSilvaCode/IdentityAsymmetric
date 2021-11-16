using IA.WebAPI.Core.User;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace IA.Identity.API.Configurations
{
    public static class DependencyInjectionConfig
    {
        public static void RegisterServices(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddScoped<Services.v1_0.AuthenticationService>();
            services.AddScoped<Services.v2_0.AuthenticationService>();
            services.AddScoped<IAspNetUser, AspNetUser>();
        }
    }
}