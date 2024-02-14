using Si.IdCheck.ApiClients.CloudCheck.Models.Responses;
using Si.IdCheck.Workers.Application.Models.Requests;
using Si.IdCheck.Workers.Application.ServiceBus;

namespace Si.IdCheck.Workers.Application.Unit.Tests.Extensions;
public class GetAssociationExtensionsTests
{
    [Fact]
    public void ToMessage_Should_Return_Equal_Values()
    {
        var clientId = Guid.NewGuid().ToString();

        var match = new Match
        {
            Peid = new Random().Next(0, 10000),
            MatchId = Guid.NewGuid().ToString(),
            RiskTypes = Enumerable.Range(0, 3).Select(x => new RiskType { Code = Guid.NewGuid().ToString() }).ToList()
        };

        var response = new GetAssociationResponse
        {
            Matches = [match],
            AssociationReference = Guid.NewGuid().ToString()
        };

        var message = response.ToServiceBusMessage(match, clientId);

        foreach (var m in response.Matches)
        {
            Assert.Equal(m.Peid, message.Peid);
            Assert.Equal(m.MatchId, message.MatchId);
            Assert.Equal(m.RiskTypes, message.RiskTypes);
        }

        Assert.Equal(clientId, message.ClientId);
        Assert.Equal(response.AssociationReference, message.AssociationReference);
        Assert.Equal(response.PersonDetail?.BirthYear, message.PersonOfInterestBirthYear);
    }

    [Fact]
    public void ToRequest_Should_Return_Equal_Values()
    {
        var message = new OngoingMonitoringAlertMessages.GetAssociation
        {
            ClientId = Guid.NewGuid().ToString(),
            AssociationReference = Guid.NewGuid().ToString()
        };

        var request = message.ToRequest();

        Assert.Equal(message.ClientId, request.ClientId);
        Assert.Equal(message.AssociationReference, request.AssociationReference);
    }
}
