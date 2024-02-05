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
        IOptions<CloudCheckSettings> cloudCheckSettingsOption,
        IOptions<ReviewMatchSettings> reviewMatchSettingsOptions)
    {
        _client = client;
        _cloudCheckSettings = cloudCheckSettingsOption.Value;
        _reviewMatchSettings = reviewMatchSettingsOptions.Value;
        _tableStorageService = tableStorageService;
    }

    public async Task<Result> Handle(ReviewMatch request, CancellationToken cancellationToken)
    {
        //todo: Rules for clearing matches;
        //Check first if it matches any of the relationships to filter.
        //If associate is empty, it means we filtered the relationships and it didn't return any result, so we can clear it now.
        if (!request.Associates.Any())
        {
            await ReviewMatchAsync(request, "No family member in relationship filter found.", cancellationToken);
        }

        //Check relationships and join the persondetails in the match associates.
        var childRelationshipTypes = new[]
        {
            CloudCheckRelationshipConsts.Son,
            CloudCheckRelationshipConsts.Daughter
        };
            
        var hasChildIssue = false;


        var personBirthYear = int.Parse(request.Association.PersonDetail.BirthYear);

        foreach (var associate in request.Associates)
        {
            //if true exit loop.
            if (hasChildIssue)
            {
                break;
            }

            foreach (var match in associate.Response.Matches)
            {
                var dateOfBirth = match
                    .Dates
                    .FirstOrDefault(x =>
                        "Date of Birth".Equals(x.Type, StringComparison.InvariantCultureIgnoreCase));


                if (dateOfBirth == null) continue;

                var childDateOfBirthYear = int.Parse(dateOfBirth.Year);

                //Condition to check if it's a child but age doesn't match
                //if true exit loop.
                if (childDateOfBirthYear >= personBirthYear) continue;
                hasChildIssue = true;
                break;
            }
        }

        if (!hasChildIssue)
        {
            await ReviewMatchAsync(request, "Has match with children but children's age is greater.", cancellationToken);
        }

        return Result.Success();
    }

    private async Task ReviewMatchAsync(ReviewMatch request, string notes, CancellationToken cancellationToken)
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
                    Notes = notes
                }
            };

            await _client.ReviewMatchAsync(cloudCheckRequest, _cloudCheckSettings.ApiKey,
                _cloudCheckSettings.ApiSecret);
        }

        var log = new ReviewMatchLogEntity(request.Association.AssociationReference, request.Match.MatchId, notes,
            _reviewMatchSettings.ClearEnabled);

        await _tableStorageService.InsertAsync(log, cancellationToken);

        Result.Success();
    }
}