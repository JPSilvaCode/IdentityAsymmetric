using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetDevPack.Security.JwtExtensions;

namespace IA.WebAPI.Core.Identity
{
    public static class JwtConfig
    {
        public static void AddJwtConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            var appSettingsSection = configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);

            var appSettings = appSettingsSection.Get<AppSettings>();

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = true;
                x.SaveToken = true;
                x.SetJwksOptions(new JwkOptions(appSettings.AutenticacaoJwksUrl));
            });
        }

        //public static void AddJwtConfiguration(this IServiceCollection services, IConfiguration configuration)
        //{
        //    var appSettingsSection = configuration.GetSection("AppSettings");
        //    services.Configure<AppSettings>(appSettingsSection);

        //    var appSettings = appSettingsSection.Get<AppSettings>();
        //    var key = Encoding.ASCII.GetBytes(appSettings.Secret);

        //    services.AddAuthentication(options =>
        //    {
        //        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        //        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        //    }).AddJwtBearer(bearerOptions =>
        //    {
        //        bearerOptions.RequireHttpsMetadata = true;
        //        bearerOptions.SaveToken = true;
        //        bearerOptions.TokenValidationParameters = new TokenValidationParameters
        //        {
        //            ValidateIssuerSigningKey = true,
        //            IssuerSigningKey = new SymmetricSecurityKey(key),
        //            ValidateIssuer = true,
        //            ValidateAudience = true,
        //            ValidAudience = appSettings.ValidoEm,
        //            ValidIssuer = appSettings.Emissor,
        //            ValidateLifetime = true,
        //            ClockSkew = TimeSpan.Zero
        //        };
        //    });
        //}

        public static void UseAuthConfiguration(this IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.UseAuthorization();
        }
    }
}
