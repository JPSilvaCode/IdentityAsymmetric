using IA.Identity.API;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", true, true);
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, true);
builder.Configuration.AddEnvironmentVariables().AddUserSecrets(Assembly.GetExecutingAssembly(), true);

var startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);

var app = builder.Build();
var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
startup.Configure(app, app.Environment, provider);
app.MapControllers();
app.Run();