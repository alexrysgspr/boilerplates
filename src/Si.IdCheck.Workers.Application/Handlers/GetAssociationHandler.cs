using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Options;
using Si.IdCheck.ApiClients.Verifidentity;
using Si.IdCheck.ApiClients.Verifidentity.Models.Requests;
using Si.IdCheck.ApiClients.Verifidentity.Models.Responses;
using Si.IdCheck.Workers.Application.Models.Requests;

namespace Si.IdCheck.Workers.Application.Handlers;
public class GetAssociationHandler : IRequestHandler<GetAssociation, Result<GetAssociationResponse>>
{
    private readonly IVerifidentityApiClient _client;
    private readonly VerifidentitySettings _verifidentitySettings;

    public GetAssociationHandler(
        IVerifidentityApiClient client,
        IOptions<VerifidentitySettings> verifidentitySettingsOption)
    {
        _client = client;
        _verifidentitySettings = verifidentitySettingsOption.Value;
    }

    public async Task<Result<GetAssociationResponse>> Handle(GetAssociation request, CancellationToken cancellationToken)
    {
        var verifidentityRequest = new GetAssociationRequest
        {
            AssociationReference = request.AssociationReference
        };

        var association = await _client.GetAssociationAsync(verifidentityRequest, _verifidentitySettings.ApiKey,
            _verifidentitySettings.ApiSecret);

        association.Matches = association
            .Matches
            .Where(x => x.Type == "RELATIONSHIP")
            .ToList();

        return Result.Success(association);
    }
}
