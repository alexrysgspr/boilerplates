using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Options;
using Si.IdCheck.ApiClients.CloudCheck;
using Si.IdCheck.ApiClients.CloudCheck.Models.Requests;
using Si.IdCheck.Workers.Application.Models.Requests;
using Si.IdCheck.Workers.Application.Models.Responses;
using Si.IdCheck.Workers.Application.Reviewers;

namespace Si.IdCheck.Workers.Application.Handlers;
public class GetMatchAssociatesPersonDetailsHandler : IRequestHandler<GetMatchAssociatesPersonDetailsRequest, Result<GetMatchAssociatesPersonDetailsResponse>>
{
    private readonly ICloudCheckApiClient _client;
    private readonly IOptionsSnapshot<ReviewerSettings> _settingsFactory;

    public GetMatchAssociatesPersonDetailsHandler(
        ICloudCheckApiClient client,
        IOptionsSnapshot<ReviewerSettings> settingsFactory)
    {
        _client = client;
        _settingsFactory = settingsFactory;
    }

    public async Task<Result<GetMatchAssociatesPersonDetailsResponse>> Handle(GetMatchAssociatesPersonDetailsRequest request, CancellationToken cancellationToken)
    {
        var settings = _settingsFactory.Get(request.ClientId);

        var response = new GetMatchAssociatesPersonDetailsResponse
        {
            AssociatesInRelationshipFilter = [],
            AssociatesNotInRelationshipFilter = []
        };

        if (request.Associates == null!) return Result.Success(response);

        foreach (var associate in request.Associates)
        {
            //Filter relationships to lookup only.
            if (!string.IsNullOrEmpty(associate.Relationship) && !settings.RelationshipsToFilter.Contains(associate.Relationship, StringComparer.InvariantCultureIgnoreCase))
            {
                response.AssociatesNotInRelationshipFilter.Add(associate);
                continue;
            }

            var cloudCheckRequest = new PeidLookupRequest
            {
                Peid = associate.Peid
            };

            var result = await _client.LookupPeidAsync(cloudCheckRequest, settings.ApiKey,
                settings.ApiSecret);

            if (result.Response.Matches == null! || !result.Response.Matches.Any())
                continue;

            response.AssociatesInRelationshipFilter.Add(result);
        }

        return Result.Success(response);
    }
}