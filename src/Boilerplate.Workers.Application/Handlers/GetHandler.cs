using Ardalis.Result;
using Ardalis.Result.FluentValidation;
using Boilerplate.Workers.Application.Models.Requests;
using Boilerplate.Workers.Application.Models.Responses;
using MediatR;
namespace Boilerplate.Workers.Application.Handlers;
public class GetHandler : IRequestHandler<Get, Result<GetResponse>>
{
    public GetHandler()
    {
    }

    public async Task<Result<GetResponse>> Handle(Get request, CancellationToken cancellationToken)
    {
        var validator = new GetValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            return Result.Invalid(validationResult.AsErrors());
        }

        return Result.Success();
    }
}
