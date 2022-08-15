using GMServer;
using GMServer.Authentication.Extensions;
using GMServer.Caching.DataFiles.Extensions;
using GMServer.Models.Settings;
using GMServer.Mongo.Extensions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

IConfiguration Configuration = builder.Configuration;
IServiceCollection services = builder.Services;

// Logging
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

services.AddLogging(opt => opt.AddSerilog());

services.AddControllers().AddNewtonsoftJson(opt => opt.UseMemberCasing());

// Swagger
services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "GMServer", Version = "v1" });
});

// MongoDB
services.AddMongo(Configuration);
services.AddMongoRepositories();

services.AddDataFileCache();
services.AddServices();
services.AddMediatR(Assembly.GetCallingAssembly());
services.AddJWTAuthentication(Configuration);
services.Configure<EncryptionSettings>(Configuration, "EncryptionSettings");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    IdentityModelEventSource.ShowPII = true;

    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "GMServer v1"));
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();