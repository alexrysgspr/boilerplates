using Azure.Messaging.ServiceBus;

namespace Si.IdCheck.Workers.Application.Unit.Tests.Mocks;
public class MockServiceBusClient : ServiceBusClient
{
    public override ServiceBusSender CreateSender(string queueOrTopicName)
    {
        return new MockServiceBusSender();
    }
}
