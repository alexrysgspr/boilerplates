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
        //Check first if it matches any of the relationships to filter.
        //If associate is empty, it means we filtered the relationships and it didn't return any result, so can clear it now.
        if (request.Associates.Any())
        {
            if (_reviewMatchSettings.ClearEnabled)
            {
                var cloudCheckRequest = new ReviewMatchRequest
                {
                    AssociationReference = request.AssociationReference,
                    Review = new Review
                    {
                        Decision = CloudCheckDecisionConsts.Cleared,
                        MatchId = request.Match.MatchId,
                        Notes = ""
                    }
                };

                await _client.ReviewMatchAsync(cloudCheckRequest, _cloudCheckSettings.ApiKey,
                    _cloudCheckSettings.ApiSecret);
            }

            var log = new ReviewMatchLogEntity(request.AssociationReference, request.Match.MatchId, "",
                _reviewMatchSettings.ClearEnabled);

            await _tableStorageService.InsertAsync(log, cancellationToken);

            return Result.Success();
        }
        

        var childRelationshipTypes = new[]
        {
            CloudCheckRelationshipConsts.Son,
            CloudCheckRelationshipConsts.Daughter
        };

        
        //Add condition to check if it's a child but age doesn't match
        //Todo: Which birthdate should we compare to?
        var children = request
            .MatchDetails
            .Associates
            .Select(x => new { x, x.Relationship })
            .Where(x => childRelationshipTypes.Contains(x.Relationship))
            .Select(x => x.x)
            .ToList();
            
        var hasChildIssue = false;

        foreach (var child in children)
        {
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
                    AssociationReference = request.AssociationReference,
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

            var log = new ReviewMatchLogEntity(request.AssociationReference, request.Match.MatchId, "",
                _reviewMatchSettings.ClearEnabled);

            await _tableStorageService.InsertAsync(log, cancellationToken);
        }

        return Result.Success();
    }
}