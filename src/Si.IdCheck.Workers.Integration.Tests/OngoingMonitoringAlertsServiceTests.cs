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
        using var scope = _factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IOngoingMonitoringAlertsService>();

        do
        {
            try
            {
                await service.DoWorkAsync("006c5ba8-35f9-4345-a584-f0e488eab328", "omg", CancellationToken.None);
            }
            catch (Exception e)
            {
                await Task.Delay(1000);
            }
        } while (true);
    }
}