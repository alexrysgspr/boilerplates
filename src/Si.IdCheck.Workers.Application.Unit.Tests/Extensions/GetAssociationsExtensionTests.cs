using Si.IdCheck.ApiClients.CloudCheck.Models.Responses;
using Si.IdCheck.Workers.Application.Models.Requests;
using Si.IdCheck.Workers.Application.ServiceBus;

namespace Si.IdCheck.Workers.Application.Unit.Tests.Extensions;
public class GetAssociationsExtensionTests
{
    [Fact]
    public void ToMessage_Should_Return_Equal_Values()
    {
        var clientId = Guid.NewGuid().ToString();

        var association = new Association
        {
            AssociationReference = Guid.NewGuid().ToString()
        };

        var response = new GetAssociationsResponse
        {
            Associations = [association]
        };

        foreach (var a in response.Associations)
        {
            var message = a.ToServiceBusMessage(clientId);
            Assert.Equal(a.AssociationReference, message.AssociationReference);
            Assert.Equal(clientId, message.ClientId);
        }
    }

    [Fact]
    public void ToRequest_Should_Return_Equal_Values()
    {
        var message = new OngoingMonitoringAlertMessages.GetAssociations
        {
            ClientId = Guid.NewGuid().ToString()
        };

        var request = message.ToRequest();

        Assert.Equal(message.ClientId, request.ClientId);
    }
}
