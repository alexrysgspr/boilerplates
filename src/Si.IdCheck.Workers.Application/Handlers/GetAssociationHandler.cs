using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Options;
using Si.IdCheck.ApiClients.CloudCheck;
using Si.IdCheck.ApiClients.CloudCheck.Models.Requests;
using Si.IdCheck.ApiClients.CloudCheck.Models.Responses;
using Si.IdCheck.Workers.Application.Models.Requests;

namespace Si.IdCheck.Workers.Application.Handlers;
public class GetAssociationHandler : IRequestHandler<GetAssociation, Result<GetAssociationResponse>>
{
    private readonly ICloudCheckApiClient _client;
    private readonly CloudCheckSettings _cloudCheckSettings;

    public GetAssociationHandler(
        ICloudCheckApiClient client,
        IOptions<CloudCheckSettings> cloudCheckSettingsOption)
    {
        _client = client;
        _cloudCheckSettings = cloudCheckSettingsOption.Value;
    }

    public async Task<Result<GetAssociationResponse>> Handle(GetAssociation request, CancellationToken cancellationToken)
    {
        var CloudCheckRequest = new GetAssociationRequest
        {
            AssociationReference = request.AssociationReference
        };

        var association = await _client.GetAssociationAsync(CloudCheckRequest, _cloudCheckSettings.ApiKey,
            _cloudCheckSettings.ApiSecret);

        association.Matches = association
            .Matches
            .Where(x => x.Type == "RELATIONSHIP")
            .ToList();

        return Result.Success(association);
    }
}
