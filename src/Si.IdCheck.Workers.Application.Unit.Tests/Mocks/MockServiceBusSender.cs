using Azure.Messaging.ServiceBus;

namespace Si.IdCheck.Workers.Application.Unit.Tests.Mocks;
public class MockServiceBusSender : ServiceBusSender
{
    public override Task SendMessageAsync(ServiceBusMessage message, CancellationToken cancellationToken = new CancellationToken())
    {
        return Task.CompletedTask;
    }
}
