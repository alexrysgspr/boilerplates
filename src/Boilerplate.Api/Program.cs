using System.Text.Json;
using Asp.Versioning;
using Boilerplate.Api.Application;
using FluentValidation;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Host.ConfigureDefaultAppConfiguration<Program>();
builder.Host.UseSerilog();

var services = builder.Services;

builder
    .Services
    .AddMediatR(mediatorConfig =>
    {
        mediatorConfig.RegisterServicesFromAssembly(typeof(ApiApplication).Assembly);
    })
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1.0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

services
    .AddRouting(o => o.LowercaseUrls = true)
    .AddHealthCheckServices()
    .AddDefaultAuth(configuration);

services
    .AddControllers(options =>
    {
        options.RespectBrowserAcceptHeader = true;
        options.Filters.Add(new AuthorizeFilter());
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });


services.AddValidatorsFromAssembly(typeof(ApiApplication).Assembly);
services.AddFluentValidationRulesToSwagger();

services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen(options =>
    {
        options
            .SwaggerDoc("v1", new OpenApiInfo { Title = "IdCheck Api", Version = "v1" });

        options.DescribeAllParametersInCamelCase();
        options.AddSwaggerAuth();
        options.IncludeXmlCommentsIfExists(typeof(Program).Assembly);
        options.IncludeXmlCommentsIfExists(typeof(ApiApplication).Assembly);
    });



var app = builder.Build();

Log.Logger = app.CreateDefaultLogger();

// Configure the HTTP request pipeline.

app
.UseSwagger(c => c.RouteTemplate = "/swagger/docs/{documentName}/swagger.json")
.UseSwaggerUI(c =>
{
    c.RoutePrefix = string.Empty;
    c.SwaggerEndpoint("/swagger/docs/v1/swagger.json", "IdCheck Api");
})
.UseHttpsRedirection()
.UseGlobalException()
.UseRouting()
.UseAuthentication()
.UseAuthorization()
.UseCustomHealthChecks(app.Configuration)
.UseEndpoints(endpoints => endpoints.MapControllers());

try
{
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
