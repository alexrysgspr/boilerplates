using Boilerplate.Api.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

// ReSharper disable CheckNamespace
namespace Microsoft.AspNetCore.Builder;
// ReSharper restore CheckNamespace

public static class AppBuilderExtensions
{
    public const string AppStatusPath = "/_appstatus";
    public const string PingPath = "/_ping";
    public static IApplicationBuilder UseCustomHealthChecks(this IApplicationBuilder app, IConfiguration configuration)
    {
        app.UseEndpoints(endpoints =>
        {
            var appStatusAccessKey = configuration["AppSettings:AppStatusAccessKey"];

            endpoints.MapHealthChecks($"{AppStatusPath}/{appStatusAccessKey}",
                new HealthCheckOptions()
                {
                    ResponseWriter = HealthCheckConfig.ResponseWriterAsync,
                    Predicate = c => c.Name.Equals(nameof(PingHealthCheck)) == false
                });

            endpoints.MapHealthChecks(PingPath, new HealthCheckOptions()
            {
                Predicate = c => c.Name.Equals(nameof(PingHealthCheck)),
                ResponseWriter = async (c, r) =>
                {
                    await c.Response.WriteAsync("pong");
                }
            });
        });

        return app;
    }
}
