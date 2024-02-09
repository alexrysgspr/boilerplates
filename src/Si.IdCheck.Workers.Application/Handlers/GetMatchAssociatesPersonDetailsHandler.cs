using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Options;
using Si.IdCheck.ApiClients.CloudCheck;
using Si.IdCheck.ApiClients.CloudCheck.Models.Requests;
using Si.IdCheck.Workers.Application.Models.Requests;
using Si.IdCheck.Workers.Application.Models.Responses;
using Si.IdCheck.Workers.Application.Settings;

namespace Si.IdCheck.Workers.Application.Handlers;
public class GetMatchAssociatesPersonDetailsHandler : IRequestHandler<GetMatchAssociatesPersonDetailsRequest, Result<GetMatchAssociatesPersonDetailsResponse>>
{
    private readonly ICloudCheckApiClient _client;
    private readonly CloudCheckSettings _cloudCheckSettings;
    private readonly ReviewMatchSettings _reviewMatchSettingsOption;

    public GetMatchAssociatesPersonDetailsHandler(
        ICloudCheckApiClient client,
        IOptions<CloudCheckSettings> cloudCheckSettingsOption,
        IOptions<ReviewMatchSettings> reviewMatchSettingsOption)
    {
        _client = client;
        _cloudCheckSettings = cloudCheckSettingsOption.Value;
        _reviewMatchSettingsOption = reviewMatchSettingsOption.Value;
    }

    public async Task<Result<GetMatchAssociatesPersonDetailsResponse>> Handle(GetMatchAssociatesPersonDetailsRequest request, CancellationToken cancellationToken)
    {
        var response = new GetMatchAssociatesPersonDetailsResponse
        {
            AssociatesInRelationshipFilter = [],
            AssociatesNotInInRelationshipFilter = []
        };

        if (request.Associates == null!) return Result.Success(response);

        foreach (var associate in request.Associates)
        {
            //Filter relationships to lookup only.
            if (!string.IsNullOrEmpty(associate.Relationship) && !_reviewMatchSettingsOption.RelationshipsToFilter.Contains(associate.Relationship, StringComparer.InvariantCultureIgnoreCase))
            {
                response.AssociatesNotInInRelationshipFilter.Add(associate);
                continue;
            }

            var cloudCheckRequest = new PeidLookupRequest
            {
                Peid = associate.Peid
            };

            var result = await _client.LookupPeidAsync(cloudCheckRequest, _cloudCheckSettings.ApiKey,
                _cloudCheckSettings.ApiSecret);

            if (result.Response.Matches == null! || !result.Response.Matches.Any())
                continue;

            response.AssociatesInRelationshipFilter.Add(result);
        }

        return Result.Success(response);
    }
}