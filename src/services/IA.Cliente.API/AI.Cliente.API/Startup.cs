using AI.Cliente.API.Configurations;
using IA.WebAPI.Core.Identity;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace AI.Cliente.API
{
	public class Startup
	{
		public IConfiguration Configuration { get; }

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}
	
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddApiConfiguration();
			services.AddJwtConfiguration(Configuration);
			services.AddVersionConfiguration();
			services.AddSwaggerConfiguration();
			services.RegisterServices();
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
		{			
			app.UseSwaggerSetup(provider);
			app.UseVersionSetup();
			app.UseApiConfiguration(env);
		}
	}
}
