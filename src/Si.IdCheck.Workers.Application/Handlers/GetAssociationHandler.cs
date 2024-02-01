using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Options;
using Si.IdCheck.ApiClients.Cloudcheck;
using Si.IdCheck.ApiClients.Cloudcheck.Models.Requests;
using Si.IdCheck.ApiClients.Cloudcheck.Models.Responses;
using Si.IdCheck.Workers.Application.Models.Requests;

namespace Si.IdCheck.Workers.Application.Handlers;
public class GetAssociationHandler : IRequestHandler<GetAssociation, Result<GetAssociationResponse>>
{
    private readonly ICloudcheckApiClient _client;
    private readonly CloudcheckSettings _CloudcheckSettings;

    public GetAssociationHandler(
        ICloudcheckApiClient client,
        IOptions<CloudcheckSettings> CloudcheckSettingsOption)
    {
        _client = client;
        _CloudcheckSettings = CloudcheckSettingsOption.Value;
    }

    public async Task<Result<GetAssociationResponse>> Handle(GetAssociation request, CancellationToken cancellationToken)
    {
        var CloudcheckRequest = new GetAssociationRequest
        {
            AssociationReference = request.AssociationReference
        };

        var association = await _client.GetAssociationAsync(CloudcheckRequest, _CloudcheckSettings.ApiKey,
            _CloudcheckSettings.ApiSecret);

        association.Matches = association
            .Matches
            .Where(x => x.Type == "RELATIONSHIP")
            .ToList();

        return Result.Success(association);
    }
}
