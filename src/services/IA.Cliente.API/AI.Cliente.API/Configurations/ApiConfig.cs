using IA.WebAPI.Core.Identity;

namespace AI.Cliente.API.Configurations
{
	public static class ApiConfig
	{
		public static void AddApiConfiguration(this IServiceCollection services)
		{
			if (services == null) throw new ArgumentNullException(nameof(services));

			services.AddCors();
			services.AddControllers();	
		}

		public static void UseApiConfiguration(this IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.UseCors(option => option
			.AllowAnyOrigin()
			.AllowAnyMethod()
			.AllowAnyHeader());

			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseAuthConfiguration();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}
