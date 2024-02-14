using Ardalis.Result;
using FluentValidation;
using MediatR;
using Si.IdCheck.ApiClients.CloudCheck.Models.Responses;

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