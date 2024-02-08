using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;

// ReSharper disable CheckNamespace
namespace Si.IdCheck.ApiClients.CloudCheck.Extensions;
// ReSharper restore CheckNamespace

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCloudCheck(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection(nameof(CloudCheckSettings));
        services.Configure<CloudCheckSettings>(settings);

        services
            .AddHttpClient<ICloudCheckApiClient, CloudCheckApiClient>(client =>
            {
                client.BaseAddress = new Uri(settings.Get<CloudCheckSettings>().BaseUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
            });
        //.AddTransientHttpErrorPolicy(builder =>
        //    builder
        //        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));

        return services;
    }
}
