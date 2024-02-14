using Ardalis.Result;
using FluentValidation;
using MediatR;
using Si.IdCheck.ApiClients.CloudCheck.Models.Responses;
using Si.IdCheck.Workers.Application.ServiceBus;

namespace Si.IdCheck.Workers.Application.Models.Requests;
public class ReviewMatch : IRequest<Result>
{
    public string ClientId { get; set; }
    public string AssociationReference { get; set; }
    public string MatchId { get; set; }
    public int? Peid { get; set; }
    public string PersonOfInterestBirthYear { get; set; }
    public List<RiskType> RiskTypes { get; set; }
}

public static class ReviewMatchExtensions
{
    public static ReviewMatch ToRequest(this OngoingMonitoringAlertMessages.ReviewMatch message)
    {
        if (message == null)
        {
            return null;
        }

        return new ReviewMatch
        {
            MatchId = message.MatchId,
            AssociationReference = message.AssociationReference,
            ClientId = message.ClientId,
            Peid = message.Peid,
            RiskTypes = message.RiskTypes,
            PersonOfInterestBirthYear = message.PersonOfInterestBirthYear
        };
    }
}

public class ReviewMatchValidator : AbstractValidator<ReviewMatch>
{
    public ReviewMatchValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty();

        RuleFor(x => x.AssociationReference)
            .NotEmpty();

        RuleFor(x => x.MatchId)
            .NotEmpty();

        RuleFor(x => x.Peid)
            .NotEmpty();
    }
}