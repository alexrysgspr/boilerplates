using Si.IdCheck.ApiClients.CloudCheck.Models.Responses;
using Si.IdCheck.Workers.Application.Models.Requests;
using Si.IdCheck.Workers.Application.ServiceBus;

namespace Si.IdCheck.Workers.Application.Unit.Tests.Extensions;
public class ReviewMatchExtensionsTests
{

    [Fact]
    public void ToRequest_Should_Return_Equal_Values()
    {
        var message = new OngoingMonitoringAlertMessages.ReviewMatch
        {
            ClientId = Guid.NewGuid().ToString(),
            Peid = new Random().Next(0, 999999),
            RiskTypes = Enumerable.Range(0, 3).Select(x => new RiskType { Code = Guid.NewGuid().ToString() }).ToList(),
            AssociationReference = Guid.NewGuid().ToString(),
            MatchId = Guid.NewGuid().ToString()
        };

        var request = message.ToRequest();

        Assert.Equal(message.MatchId, request.MatchId);
        Assert.Equal(message.AssociationReference, request.AssociationReference);
        Assert.Equal(message.ClientId, request.ClientId);
        Assert.Equal(message.Peid, request.Peid);
        Assert.Equal(message.RiskTypes, request.RiskTypes);
        Assert.Equal(message.PersonOfInterestBirthYear, request.PersonOfInterestBirthYear);
    }
}
