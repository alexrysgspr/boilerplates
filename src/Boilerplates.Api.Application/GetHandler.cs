using Ardalis.Result;
using Ardalis.Result.FluentValidation;
using Boilerplate.Api.Application.Models.Requests;
using Boilerplate.Api.Application.Models.Responses;
using Boilerplate.ApiClients.Boilerplate;
using FluentValidation;
using MediatR;

namespace Boilerplate.Api.Application;
public class GetHandler : IRequestHandler<Get, Result<GetResponse>>
{
    private readonly IBoilerplateApiClient _client;
    private readonly IValidator<Get> _validator;

    public GetHandler(
        IBoilerplateApiClient client,
        IValidator<Get> validator)
    {
        _client = client;
        _validator = validator;
    }

    public async Task<Result<GetResponse>> Handle(Get request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            return Result.Invalid(validationResult.AsErrors());
        }

        return Result.Success();
    }
}
