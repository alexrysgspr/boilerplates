using Ardalis.Result;
using Boilerplate.Workers.Application.Models.Responses;
using FluentValidation;
using MediatR;

namespace Boilerplate.Workers.Application.Models.Requests;
public class Get : IRequest<Result<GetResponse>>
{
}

public class GetValidator : AbstractValidator<Get>
{
    public GetValidator()
    {
    }
}