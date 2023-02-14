using System;
using IA.Identity.API.Services;
using IA.WebAPI.Core.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IA.Identity.API.Configurations
{
    public static class EmailConfig
    {
        public static void AddEmailConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddTransient<IEmailService, EmailService>();
            services.Configure<EmailSetting>(configuration.GetSection("EmailSettings"));
        }
    }
}