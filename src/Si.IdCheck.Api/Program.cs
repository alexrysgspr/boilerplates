using Microsoft.AspNetCore.Mvc.Authorization;
using Serilog;
using Si.IdCheck.Api.Auth;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Host.ConfigureDefaultAppConfiguration<Program>();
builder.Host.UseSerilog();

var services = builder.Services;

builder
    .Services
    .AddMediatR(mediatorConfig =>
{
    //todo
    mediatorConfig.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

services
    .AddRouting(o => o.LowercaseUrls = true)
    .AddHealthCheckServices()
    .AddDefaultAuth(configuration);

services
    .AddControllers(options =>
    {
        options.RespectBrowserAcceptHeader = true;
        options.Filters.Add(new AuthorizeFilter(AuthConstants.AuthPolicies.ApiClientPolicy));
    });

services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen();

var app = builder.Build();

Log.Logger = app.CreateDefaultLogger();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app
    .UseHttpsRedirection()
    .UseRouting()
    .UseAuthentication()
    .UseAuthorization()
    .UseCustomHealthChecks(app.Configuration)
    .UseEndpoints(endpoints => endpoints.MapControllers());

try
{
    throw new Exception("dsadasda");
    await app.RunAsync();
    return 0;
}
catch (Exception e)
{
    Log.Fatal(e, "Host terminated unexpectedly, check the application's host configuration.");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}
public partial class Program
{
}