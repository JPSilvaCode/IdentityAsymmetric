using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.DependencyInjection;

namespace IA.Identity.API.Configurations
{
    public static class VersionConfig
    {
        public static void AddVersionConfiguration(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddApiVersioning(p =>
            {
                p.ApiVersionReader = new UrlSegmentApiVersionReader();
                p.DefaultApiVersion = new ApiVersion(2, 0);
                p.ReportApiVersions = true;
                p.AssumeDefaultVersionWhenUnspecified = true;
                p.ApiVersionSelector = new CurrentImplementationApiVersionSelector(p);
            });

            services.AddVersionedApiExplorer(p =>
            {
                p.GroupNameFormat = "'v'VVV";
                p.SubstituteApiVersionInUrl = true;
            });

        }

        public static void UseVersionSetup(this IApplicationBuilder app)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            app.UseApiVersioning();
        }
    }
}