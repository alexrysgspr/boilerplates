using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Options;
using Si.IdCheck.ApiClients.CloudCheck;
using Si.IdCheck.ApiClients.CloudCheck.Models.Requests;
using Si.IdCheck.ApiClients.CloudCheck.Models.Responses;
using Si.IdCheck.Workers.Application.Models.Requests;
using Si.IdCheck.Workers.Application.Settings;

namespace Si.IdCheck.Workers.Application.Handlers;
public class GetMatchAssociatesPeidDetailsHandler : IRequestHandler<GetMatchAssociatesPeidDetailsRequest, Result<List<PeidLookupResponse>>>
{
    private readonly ICloudCheckApiClient _client;
    private readonly CloudCheckSettings _cloudCheckSettings;
    private readonly ReviewMatchSettings _reviewMatchSettingsOption;

    public GetMatchAssociatesPeidDetailsHandler(
        ICloudCheckApiClient client,
        IOptions<CloudCheckSettings> cloudCheckSettingsOption,
        IOptions<ReviewMatchSettings> reviewMatchSettingsOption)
    {
        _client = client;
        _cloudCheckSettings = cloudCheckSettingsOption.Value;
        _reviewMatchSettingsOption = reviewMatchSettingsOption.Value;
    }

    public async Task<Result<List<PeidLookupResponse>>> Handle(GetMatchAssociatesPeidDetailsRequest request, CancellationToken cancellationToken)
    {
        var results = new List<PeidLookupResponse>();

        if (request.Associates == null!) return Result.Success(results);

        foreach (var associate in request.Associates)
        {
            if (!_reviewMatchSettingsOption.RelationshipsToFiltler.Contains(associate.Relationship))
            {
                continue;
            }

            var cloudCheckRequest = new PeidLookupRequest
            {
                Peid = associate.Peid
            };

            var response = await _client.LookupPeidAsync(cloudCheckRequest, _cloudCheckSettings.ApiKey,
                _cloudCheckSettings.ApiSecret);

            results.Add(response);
        }

        return Result.Success(results);
    }
}