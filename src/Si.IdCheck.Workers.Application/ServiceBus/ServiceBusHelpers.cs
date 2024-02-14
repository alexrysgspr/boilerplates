using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;

namespace Si.IdCheck.Workers.Application.ServiceBus;
public static class ServiceBusHelpers
{
    public static ServiceBusMessage CreateMessage<T>(T body, string subject)
    {
        return new ServiceBusMessage
        {
            Subject = subject,
            Body = new BinaryData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(body)))
        };
    }
}
