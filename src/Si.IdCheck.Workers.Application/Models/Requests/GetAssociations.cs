using Ardalis.Result;
using FluentValidation;
using MediatR;
using Si.IdCheck.ApiClients.CloudCheck.Models.Responses;
using Si.IdCheck.Workers.Application.ServiceBus;

namespace Si.IdCheck.Workers.Application.Models.Requests;
public class GetAssociations : IRequest<Result>
{
    public string ClientId { get; set; }
}

public static class GetAssociationsExtensions 
{
    public static OngoingMonitoringAlertMessages.GetAssociation ToServiceBusMessage(this Association response, string clientId)
    {
        if (response == null)
        {
            return null;
        };

        return new OngoingMonitoringAlertMessages.GetAssociation
        {
            AssociationReference = response.AssociationReference,
            ClientId = clientId
        };
    }

    public static GetAssociations ToRequest(this OngoingMonitoringAlertMessages.GetAssociations message)
    {
        if (message == null)
        {
            return null;
        };

        return new GetAssociations
        {
            ClientId = message.ClientId
        };
    }
}

public class GetAssociationsValidator : AbstractValidator<GetAssociations>
{
    public GetAssociationsValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty();
    }
}