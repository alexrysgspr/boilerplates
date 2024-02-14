using Ardalis.Result;
using FluentValidation;
using MediatR;
using Si.IdCheck.ApiClients.CloudCheck.Models.Responses;
using Si.IdCheck.Workers.Application.ServiceBus;

namespace Si.IdCheck.Workers.Application.Models.Requests;
public class GetAssociation : IRequest<Result<GetAssociationResponse>>
{
    public string AssociationReference { get; set; }
    public string ClientId { get; set; }
}

public static class GetAssociationExtensions
{
    public static OngoingMonitoringAlertMessages.ReviewMatch ToServiceBusMessage(this GetAssociationResponse response, Match match, string clientId)
    {
        if (response == null)
        {
            return null;
        };

        return new OngoingMonitoringAlertMessages.ReviewMatch
        {
            AssociationReference = response.AssociationReference,
            Peid = match.Peid,
            ClientId = clientId,
            MatchId = match.MatchId,
            PersonOfInterestBirthYear = response.PersonDetail?.BirthYear,
            RiskTypes = match.RiskTypes
        };
    }

    public static GetAssociation ToRequest(this OngoingMonitoringAlertMessages.GetAssociation message)
    {
        if (message == null)
        {
            return null;
        };

        return new GetAssociation
        {
            ClientId = message.ClientId,
            AssociationReference = message.AssociationReference
        };
    }
}

public class GetAssociationValidator : AbstractValidator<GetAssociation>
{
    public GetAssociationValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty();
        RuleFor(x => x.AssociationReference)
            .NotEmpty();
    }
}