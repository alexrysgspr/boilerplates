using Ardalis.Result;
using FluentValidation;
using MediatR;

namespace Si.IdCheck.Workers.Application.Models.Requests;
public class GetAssociations : IRequest<Result>
{
    public string ClientId { get; set; }
}

public class GetAssociationsValidator : AbstractValidator<GetAssociations>
{
    public GetAssociationsValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty();
    }
}