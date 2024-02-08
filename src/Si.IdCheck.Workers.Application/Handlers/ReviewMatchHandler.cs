using System.Globalization;
using System.Text.Json;
using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Options;
using Serilog;
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
    private static readonly ILogger Logger = Log.ForContext<ReviewMatchHandler>();
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
        if (!request.MatchAssociates.AssociatesInRelationshipFilter.Any())
        {
            var associatesNotInRelationshipFilter = request
                .MatchAssociates
                .AssociatesNotInInRelationshipFilter
                .Select(x => new { x.Peid, x.Relationship, x.Description1})
                .ToList();

            await ReviewMatchAsync(request, $"No family member in relationship filter found. Associates: {JsonSerializer.Serialize(associatesNotInRelationshipFilter)}, AssociationReference: {request.PersonOfInterest.AssociationReference}, MatchId: {request.Match.MatchId}, Peid: {request.Match.Peid}. RiskType: RCA.", cancellationToken);
            return Result.Success();
        }

        //Check relationships and join the persondetails in the match associates.

        var dobType = "Date of Birth";

        var associatesDetails = request
            .MatchAssociates
            .AssociatesInRelationshipFilter
            .SelectMany(x => x.Response.Matches)
            .Select(x => x)
            .ToList();


        var associates = request
            .MatchDetails
            .Associates
            .Join(associatesDetails, associateDetails => associateDetails.Peid, matchDetails => matchDetails.Peid, (associateDetails, associate) => new
            {
                associateDetails.Peid,
                associateDetails.Relationship,
                DateOfBirth = associate.Dates.FirstOrDefault(x => dobType.Equals(x.Type, StringComparison.InvariantCultureIgnoreCase))
            })
            .ToList();

        var personOfInterestBirthYear = int.Parse(request.PersonOfInterest.PersonDetail.BirthYear);
        var hasIssue = false;
        var notes = new List<string>();
        foreach (var associate in associates)
        {
            //if true exit loop.
            if (hasIssue)
            {
                break;
            }

            if (CloudCheckRelationshipConsts.Father.Equals(associate.Relationship, StringComparison.InvariantCultureIgnoreCase)
                || CloudCheckRelationshipConsts.Mother.Equals(associate.Relationship, StringComparison.InvariantCultureIgnoreCase))
            {
                var birthYear = 0;

                if (DateTime.TryParseExact(associate.DateOfBirth?.Date,
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var birthdate) || int.TryParse(associate.DateOfBirth?.Year, out birthYear))
                {
                    if (birthdate.Year > personOfInterestBirthYear || birthYear > personOfInterestBirthYear)
                    {
                        notes.Add($"Person of interest's year of birth is '{personOfInterestBirthYear}' but the match's '{associate.Relationship}' year of birth is '{birthdate.Year}'. AssociationReference: {request.PersonOfInterest.AssociationReference}, MatchId: {request.Match.MatchId}, Peid: {request.Match.Peid}. RiskType: RCA.");
                        continue;
                    }
                }

                hasIssue = true;
                break;
            }

            if (CloudCheckRelationshipConsts.Son.Equals(associate.Relationship, StringComparison.InvariantCultureIgnoreCase) ||
                CloudCheckRelationshipConsts.Daughter.Equals(associate.Relationship, StringComparison.InvariantCultureIgnoreCase))
            {
                //Condition to check if it's a child but birth year is lesser than person of interest's birth year
                var birthYear = 0;
                if (DateTime.TryParseExact(associate.DateOfBirth?.Date,
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var birthdate) || int.TryParse(associate.DateOfBirth?.Year, out birthYear))
                {
                    if (birthdate.Year < personOfInterestBirthYear || birthdate.Year < birthYear)
                    {
                        notes.Add($"Person of interest's year of birth is '{personOfInterestBirthYear}' but the match's '{associate.Relationship}' year of birth is '{birthdate.Year}'. AssociationReference: {request.PersonOfInterest.AssociationReference}, MatchId: {request.Match.MatchId}, Peid: {request.Match.Peid}. RiskType: RCA.");

                        continue;
                    }
                }

                hasIssue = true;
                break;
            }
        }

        if (!hasIssue)
        {
            await ReviewMatchAsync(request, string.Join('\n', notes), cancellationToken);
        }

        return Result.Success();
    }

    private async Task ReviewMatchAsync(ReviewMatch request, string notes, CancellationToken cancellationToken)
    {
        if (_reviewMatchSettings.ClearEnabled)
        {
            var cloudCheckRequest = new ReviewMatchRequest
            {
                AssociationReference = request.PersonOfInterest.AssociationReference,
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

        var log = new ReviewMatchLogEntity(request.PersonOfInterest.AssociationReference, request.Match.MatchId, notes,
            _reviewMatchSettings.ClearEnabled);

        try
        {
            await _tableStorageService.InsertOrMergeAsync(log, cancellationToken);
        }
        catch (Exception e)
        {
            Logger.Error(e, $"An error occured while saving review match log. ${JsonSerializer.Serialize(log)}");
        }

        Result.Success();
    }
}