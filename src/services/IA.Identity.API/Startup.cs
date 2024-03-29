using IA.Identity.API.Configurations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IA.Identity.API
{
	public class Startup
	{
		public IConfiguration Configuration { get; }

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{			
			services.AddJwksConfiguration();
			services.AddIdentityConfiguration(Configuration);
			services.AddApiConfiguration();
			services.AddVersionConfiguration();
			services.AddDatabaseConfiguration(Configuration);
			services.AddEmailConfiguration(Configuration);
			services.AddSwaggerConfiguration();
			services.RegisterServices();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
		{			
			app.UseApiConfiguration(env);
			app.UseJwksConfiguration();
			app.UseSwaggerSetup(provider);
			app.UseVersionSetup();
		}
	}
}
