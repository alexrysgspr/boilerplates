using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Boilerplate.Workers.HealthChecks
{
    public static class HealthCheckConfig
    {
        public static async Task ResponseWriterAsync(HttpContext c, HealthReport r)
        {
            var response = new HealthCheckResponse
            {
                Status = r.Status.ToString(),
                HealthCheckEntries = r.Entries.Select(x =>
                    new HealthCheckEntry
                    {
                        Name = x.Key,
                        Status = x.Value.Status.ToString(),
                        Errors = x.Value.Data.ContainsKey("errors") ? x.Value.Data["errors"] : null
                    }).ToArray()
            };

            await c.Response.WriteAsync(
                JsonSerializer.Serialize(
                    response,
                    new JsonSerializerOptions()
                    {
                        PropertyNameCaseInsensitive = true
                    }));
        }
    }
}
