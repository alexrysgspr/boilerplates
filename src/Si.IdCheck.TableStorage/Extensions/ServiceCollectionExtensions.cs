using Azure.Data.Tables;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Si.IdCheck.AzureTableStorage;
using Si.IdCheck.AzureTableStorage.Models;
// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;
// ReSharper restore CheckNamespace

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTableStorageServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddAzureClients(builder =>
            {
                builder
                    .AddTableServiceClient(configuration.GetConnectionString(TableStorageConsts.ConnectionStringName));
            });

        services
            .AddSingleton<IAzureTableStorageService<ReviewMatchLogEntity>>(provider =>
            new AzureTableStorageService<ReviewMatchLogEntity>(provider.GetRequiredService<TableServiceClient>(), ReviewMatchLogConsts.TableName));

        return services;
    }
}