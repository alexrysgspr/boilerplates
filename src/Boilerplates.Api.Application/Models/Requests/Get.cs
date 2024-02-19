using Ardalis.Result;
using Boilerplate.Api.Application.Models.Responses;
using FluentValidation;
using MediatR;

namespace Boilerplate.Api.Application.Models.Requests;
public class Get : IRequest<Result<GetResponse>>
{
}

public class GetValidator : AbstractValidator<Get>
{
    public GetValidator()
    {
    }
}