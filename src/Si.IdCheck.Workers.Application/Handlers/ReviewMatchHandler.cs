using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Options;
using Si.IdCheck.ApiClients.CloudCheck;
using Si.IdCheck.ApiClients.CloudCheck.Constants;
using Si.IdCheck.ApiClients.CloudCheck.Models.Requests;
using Si.IdCheck.AzureTableStorage;
using Si.IdCheck.AzureTableStorage.Models;
using Si.IdCheck.Workers.Application.Models.Requests;
using Si.IdCheck.Workers.Application.Settings;

namespace Si.IdCheck.Workers.Application.Handlers;
public class ReviewMatchHandler : IRequestHandler<ReviewMatch, Result>
{
    private readonly ICloudCheckApiClient _client;
    private readonly CloudCheckSettings _cloudCheckSettings;
    private readonly ReviewMatchSettings _reviewMatchSettings;
    private readonly IAzureTableStorageService<ReviewMatchLogEntity> _tableStorageService;

    public ReviewMatchHandler(
        ICloudCheckApiClient client,
        IAzureTableStorageService<ReviewMatchLogEntity> tableStorageService,
        IOptions<CloudCheckSettings> CloudCheckSettingsOption,
        IOptions<ReviewMatchSettings> reviewMatchSettingsOptions)
    {
        _client = client;
        _cloudCheckSettings = CloudCheckSettingsOption.Value;
        _reviewMatchSettings = reviewMatchSettingsOptions.Value;
        _tableStorageService = tableStorageService;
    }

    public async Task<Result> Handle(ReviewMatch request, CancellationToken cancellationToken)
    {
        //todo: Rules for clearing matches;
        var matchFromLookup =
            request.Peid.Response.Matches.FirstOrDefault(x => x.Peid == request.Match.Peid);

        var isCleared = true;

        //Check first if it matches any of the relationships to filter.
        if (matchFromLookup != null)
        {    
            var associateRelationships = matchFromLookup
                .Associates.Select(x => x.Relationship).ToList();

            foreach (var relationship in _reviewMatchSettings.RelationshipsToFiltler)
            {
                if (associateRelationships.Any(x => relationship.Equals(x, StringComparison.InvariantCultureIgnoreCase)))
                {
                    isCleared = false;
                }
            }
        }
        else
        {
            return Result.Success();
        }

        var childRelationshipTypes = new[]
        {
            CloudCheckRelationshipConsts.Son,
            CloudCheckRelationshipConsts.Daughter
        };

        if (!isCleared)
        {
            //Add condition to check if it's a child but age doesn't match, then set isCleared to true.
            var children = matchFromLookup
                .Associates
                .Where(x => childRelationshipTypes.Contains(x.Relationship, StringComparer.CurrentCultureIgnoreCase))
                .ToList();

            var hasChildIssue = false;

            foreach (var child in children)
            {
                //Looks like we need an extra call to lookup peid to check their birthdate.
                if (true)
                {
                    hasChildIssue = true;
                }
            }

            if (!hasChildIssue)
            {
                if (_reviewMatchSettings.ClearEnabled)
                {
                    var cloudCheckRequest = new ReviewMatchRequest
                    {
                        AssociationReference = request.Association.AssociationReference,
                        Review = new Review
                        {
                            Decision = CloudCheckDecisionConsts.Cleared,
                            MatchId = request.Match.MatchId,
                            Notes = ""
                        }
                    };

                    var response = await _client.ReviewMatchAsync(cloudCheckRequest, _cloudCheckSettings.ApiKey,
                        _cloudCheckSettings.ApiSecret);
                }

                var log = new ReviewMatchLogEntity(request.Association.AssociationReference, request.Match.MatchId, "",
                    _reviewMatchSettings.ClearEnabled);

                await _tableStorageService.InsertAsync(log, cancellationToken);
            }
        }

        return Result.Success();
    }
}