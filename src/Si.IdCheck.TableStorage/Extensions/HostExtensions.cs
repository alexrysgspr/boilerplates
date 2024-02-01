using Microsoft.Extensions.DependencyInjection;
using Si.IdCheck.AzureTableStorage;
using Si.IdCheck.AzureTableStorage.Models;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.Hosting;
// ReSharper restore CheckNamespace

public static class HostExtensions
{
    public static async Task SetupTableStorageServices(this IHost host)
    {
        await host?
            .Services?
            .GetService<IAzureTableStorageService<ReviewMatchLogEntity>>()?
            .SetupAsync()!;
    }
}
