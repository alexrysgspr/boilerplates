using Microsoft.Extensions.Options;
using Si.IdCheck.ApiClients.CloudCheck;
using Si.IdCheck.ApiClients.CloudCheck.Constants;
using Si.IdCheck.ApiClients.CloudCheck.Models.Requests;
using Si.IdCheck.AzureTableStorage;
using Si.IdCheck.AzureTableStorage.Models;
using Si.IdCheck.Workers.Application.Models.Requests;
using System.Globalization;
using System.Text.Json;
using Serilog;
using Si.IdCheck.ApiClients.CloudCheck.Models.Responses;

namespace Si.IdCheck.Workers.Application.Reviewers.Omg;
public class OmgReviewer : IReviewer
{
    private readonly ICloudCheckApiClient _client;
    private readonly IAzureTableStorageService<ReviewMatchLogEntity> _tableStorageService;
    private static readonly ILogger Logger = Log.ForContext<OmgReviewer>();
    private readonly ReviewerSettings _settings;

    public OmgReviewer(
        ICloudCheckApiClient client,
        IOptionsFactory<ReviewerSettings> settingsFactory,
        IAzureTableStorageService<ReviewMatchLogEntity> tableStorageService)
    {
        _settings = settingsFactory.Create("omg");
        _client = client;
        _tableStorageService = tableStorageService;
    }

    public async Task ReviewAsync(ReviewMatch request, CancellationToken cancellationToken)
    {
        var matchDetailsRequest = new PeidLookupRequest
        {
            Peid = request.Peid.GetValueOrDefault()
        };

        var matchDetailsResult = await _client.LookupPeidAsync(matchDetailsRequest, _settings.ApiKey,
            _settings.ApiSecret);

        var matches = matchDetailsResult
            .Response
            .Matches ?? new List<MatchDetails>();

        var hasIssue = !int.TryParse(request.PersonOfInterestBirthYear, out var personOfInterestBirthYear);
        var notes = new List<string>();
        foreach (var matchPersonDetails in matches)
        {
            if (hasIssue)
                break;

            if (matchPersonDetails.Associates is null or { Count: 0 })
            {
                await ReviewMatchAsync(request, $"No associates found. AssociationReference: {request.AssociationReference}, MatchId: {request.MatchId}, Peid: {request.Peid}. RiskType: RCA.", cancellationToken);

                return;
            };

            var (associatesInRelationshipFilter, associatesNotInRelationshipFilter) =
                await GetAssociatesAsync(matchPersonDetails);

            //Check first if it matches any of the relationships to filter.
            //If associate is empty, it means we filtered the relationships and it didn't return any result, so we can clear it now.
            if (!associatesInRelationshipFilter.Any())
            {
                var associatesNotInFilter = associatesNotInRelationshipFilter
                .Select(x => new { x.Peid, x.Relationship, x.Description1 })
                .ToList();

                await ReviewMatchAsync(request, $"No associate found in the relationship filter. Associates: {JsonSerializer.Serialize(associatesNotInFilter)}, AssociationReference: {request.AssociationReference}, MatchId: {request.MatchId}, Peid: {request.Peid}. RiskType: RCA.", cancellationToken);

                return;
            }

            //Check relationships and join the person details in the match associates.
            var dobType = "Date of Birth";

            var associatesDetails = associatesInRelationshipFilter
                .SelectMany(x => x.Response.Matches)
                .Select(x => x)
                .ToList();

            var associates = matchPersonDetails
                .Associates
                .Join(associatesDetails, associateDetails => associateDetails.Peid, matchDetails => matchDetails.Peid, (associateDetails, associate) => new
                {
                    associateDetails.Peid,
                    associateDetails.Relationship,
                    DateOfBirth = associate.Dates?.FirstOrDefault(x => dobType.Equals(x.Type, StringComparison.InvariantCultureIgnoreCase))
                })
                .ToList();

            var (issue, issueNotes) = CheckIfAssociatesHaveIssues(personOfInterestBirthYear, request.Peid, request.MatchId, request.AssociationReference, associates);

            hasIssue = issue;

            if (hasIssue)
                break;

            notes.AddRange(issueNotes);
        }

        if (!hasIssue)
        {
            await ReviewMatchAsync(request, string.Join('\n', notes), cancellationToken);
        }
    }

    private Tuple<bool, List<string>> CheckIfAssociatesHaveIssues(int personOfInterestBirthYear, int? peid, string matchId, string associationReference, dynamic associates)
    {
        var hasIssue = false;
        var notes = new List<string>();

        foreach (var associate in associates)
        {
            var relationship = (string)associate.Relationship;
            if (_settings.RelationshipsToFilter.Contains(relationship, StringComparer.InvariantCultureIgnoreCase) &&
                !CloudCheckRelationshipConsts.Father.Equals(relationship, StringComparison.InvariantCultureIgnoreCase) &&
                !CloudCheckRelationshipConsts.Mother.Equals(relationship, StringComparison.InvariantCultureIgnoreCase) &&
                !CloudCheckRelationshipConsts.Son.Equals(relationship, StringComparison.InvariantCultureIgnoreCase) &&
                !CloudCheckRelationshipConsts.Daughter.Equals(relationship, StringComparison.InvariantCultureIgnoreCase))
            {
                hasIssue = true;
                break;
            }

            if (CloudCheckRelationshipConsts.Father.Equals(associate.Relationship, StringComparison.InvariantCultureIgnoreCase)
                || CloudCheckRelationshipConsts.Mother.Equals(associate.Relationship, StringComparison.InvariantCultureIgnoreCase))
            {
                var associateBirthDate = (string)associate.DateOfBirth?.Date;
                if (DateTime.TryParseExact(associateBirthDate,
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var birthdate) && birthdate.Year > personOfInterestBirthYear)
                {
                    notes.Add($"Person of interest's year of birth is '{personOfInterestBirthYear}' but the match's '{associate.Relationship}' year of birth is '{birthdate.Year}'. AssociationReference: {associationReference}, MatchId: {matchId}, Peid: {peid}. RiskType: RCA.");
                    continue;
                }

                var associateBirthYear = (string)associate.DateOfBirth?.Year;
                if (int.TryParse(associateBirthYear, out var birthYear) && birthYear > personOfInterestBirthYear)
                {
                    notes.Add($"Person of interest's year of birth is '{personOfInterestBirthYear}' but the match's '{associate.Relationship}' year of birth is '{birthYear}'. AssociationReference: {associationReference}, MatchId: {matchId}, Peid: {peid}. RiskType: RCA.");
                    continue;
                }
                hasIssue = true;
                break;
            }

            if (CloudCheckRelationshipConsts.Son.Equals(associate.Relationship, StringComparison.InvariantCultureIgnoreCase) ||
            CloudCheckRelationshipConsts.Daughter.Equals(associate.Relationship, StringComparison.InvariantCultureIgnoreCase))
            {
                //Condition to check if it's a child but birth year is lesser than person of interest's birth year
                var associateBirthDate = (string)associate.DateOfBirth?.Date;
                if (DateTime.TryParseExact(associateBirthDate,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                        out var birthdate) && birthdate.Year < personOfInterestBirthYear)
                {
                    notes.Add($"Person of interest's year of birth is '{personOfInterestBirthYear}' but the match's '{associate.Relationship}' year of birth is '{birthdate.Year}'. AssociationReference: {associationReference}, MatchId: {matchId}, Peid: {peid}. RiskType: RCA.");
                    continue;
                }

                var associateBirthYear = (string)associate.DateOfBirth?.Year;
                if (int.TryParse(associateBirthYear, out var birthYear) && birthYear < personOfInterestBirthYear)
                {
                    notes.Add($"Person of interest's year of birth is '{personOfInterestBirthYear}' but the match's '{associate.Relationship}' year of birth is '{birthYear}'. AssociationReference: {associationReference}, MatchId: {matchId}, Peid: {peid}. RiskType: RCA.");
                    continue;
                }
                hasIssue = true;
                break;
            }
        }

        return Tuple.Create(hasIssue, notes);
    }

    private async Task<Tuple<List<PeidLookupResponse>, List<AssociateDetails>>> GetAssociatesAsync(MatchDetails matchPersonDetails)
    {
        List<PeidLookupResponse> associatesInRelationshipFilter = [];
        List<AssociateDetails> associatesNotInRelationshipFilter = [];
        //Get details of the match's associate
        foreach (var associate in matchPersonDetails.Associates)
        {
            //Filter relationships to lookup only.
            if (!string.IsNullOrEmpty(associate.Relationship) && !_settings.RelationshipsToFilter.Contains(associate.Relationship, StringComparer.InvariantCultureIgnoreCase))
            {
                associatesNotInRelationshipFilter.Add(associate);
                continue;
            }

            var cloudCheckRequest = new PeidLookupRequest
            {
                Peid = associate.Peid
            };

            var result = await _client.LookupPeidAsync(cloudCheckRequest, _settings.ApiKey,
                _settings.ApiSecret);

            if (result.Response.Matches is null or { Count: 0 })
                continue;

            associatesInRelationshipFilter.Add(result);
        }

        return Tuple.Create(associatesInRelationshipFilter, associatesNotInRelationshipFilter);
    }

    private async Task ReviewMatchAsync(ReviewMatch request, string notes, CancellationToken cancellationToken)
    {
        if (_settings.ClearEnabled)
        {
            var cloudCheckRequest = new ReviewMatchRequest
            {
                AssociationReference = request.AssociationReference,
                Review = new Review
                {
                    Decision = CloudCheckDecisionConsts.Cleared,
                    MatchId = request.MatchId,
                    Notes = notes
                }
            };

            await _client.ReviewMatchAsync(cloudCheckRequest, _settings.ApiKey,
                _settings.ApiSecret);
        }

        var log = new ReviewMatchLogEntity(request.AssociationReference, request.MatchId, notes,
            _settings.ClearEnabled);

        try
        {
            await _tableStorageService.InsertOrMergeAsync(log, cancellationToken);
        }
        catch (Exception e)
        {
            Logger.Error(e, $"An error occured while saving review match log. ${JsonSerializer.Serialize(log)}");
            throw;
        }
    }
}
