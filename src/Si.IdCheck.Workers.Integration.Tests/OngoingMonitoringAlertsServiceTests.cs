using Microsoft.Extensions.DependencyInjection;
using Si.IdCheck.Workers.Services;

namespace Si.IdCheck.Workers.Integration.Tests;

public class OngoingMonitoringAlertsServiceTests : IClassFixture<FactoryBase>
{
    private readonly FactoryBase _factory;

    public OngoingMonitoringAlertsServiceTests(FactoryBase factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Tests1()
    {
        var service = _factory.Services.GetService<IOngoingMonitoringAlertsService>()!;

        await service.DoWorkAsync("025a03c8-f618-4624-abd8-bd534e267a28", CancellationToken.None);
    }
}