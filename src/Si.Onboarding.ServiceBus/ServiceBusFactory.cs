using Azure.Messaging.ServiceBus;

namespace Si.Onboarding.ServiceBus;


public interface IServiceBusFactory
{
    ServiceBusProcessor CreateProcessor(string queueName);
}

public class ServiceBusFactory : IServiceBusFactory
{
    private readonly ServiceBusClient _client;

    public ServiceBusFactory(ServiceBusClient client)
    {
        _client = client;
    }

    public ServiceBusProcessor CreateProcessor(string queueName)
    {
        var processor = _client.CreateProcessor(queueName, new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = true,
            MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(5),
            MaxConcurrentCalls = 1
        });

        return processor;
    }
}
