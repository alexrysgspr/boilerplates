using Ardalis.Result;
using FluentValidation;
using MediatR;
using Si.IdCheck.ApiClients.CloudCheck.Models.Responses;

namespace Si.IdCheck.Workers.Application.Models.Requests;
public class GetAssociation : IRequest<Result<GetAssociationResponse>>
{
    public string AssociationReference { get; set; }
    public string ClientId { get; set; }
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