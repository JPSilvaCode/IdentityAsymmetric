using IA.Identity.API.Data;
using IA.Identity.API.Extensions;
using IA.Identity.API.Identity;
using IA.Identity.API.Identity.Policies.User;
using IA.WebAPI.Core.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace IA.Identity.API.Configurations
{
    public static class IdentityConfig
    {
        public static void AddIdentityConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddDefaultIdentity<ApplicationUser>(options =>
                {
                    options.SignIn.RequireConfirmedAccount = true;
                })
                .AddRoles<ApplicationRole>()
                .AddErrorDescriber<IdentityMensagensPortugues>()
                .AddEntityFrameworkStores<IAContext>()
                .AddDefaultTokenProviders();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("DeleteUserPolicy", policy =>
                    policy.Requirements.Add(new DeleteUserRequirement("D")));
            });

            services.Configure<AppTokenSettings>(configuration.GetSection("AppTokenSettings"));

            services.AddJwtConfiguration(configuration);

            services.AddSingleton<IAuthorizationHandler, DeleteUserRequirementHandler>();
        }
    }
}