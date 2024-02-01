using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Options;
using Si.IdCheck.ApiClients.Cloudcheck;
using Si.IdCheck.ApiClients.Cloudcheck.Constants;
using Si.IdCheck.ApiClients.Cloudcheck.Models.Requests;
using Si.IdCheck.AzureTableStorage;
using Si.IdCheck.AzureTableStorage.Models;
using Si.IdCheck.Workers.Application.Models.Requests;
using Si.IdCheck.Workers.Application.Settings;

namespace Si.IdCheck.Workers.Application.Handlers;
public class ReviewMatchHandler : IRequestHandler<ReviewMatch, Result>
{
    private readonly ICloudcheckApiClient _client;
    private readonly CloudcheckSettings _CloudcheckSettings;
    private readonly ReviewMatchSettings _reviewMatchSettings;
    private readonly IAzureTableStorageService<ReviewMatchLogEntity> _tableStorageService;

    public ReviewMatchHandler(
        ICloudcheckApiClient client,
        IAzureTableStorageService<ReviewMatchLogEntity> tableStorageService,
        IOptions<CloudcheckSettings> CloudcheckSettingsOption,
        IOptions<ReviewMatchSettings> reviewMatchSettingsOptions)
    {
        _client = client;
        _CloudcheckSettings = CloudcheckSettingsOption.Value;
        _reviewMatchSettings = reviewMatchSettingsOptions.Value;
        _tableStorageService = tableStorageService;
    }

    public async Task<Result> Handle(ReviewMatch request, CancellationToken cancellationToken)
    {
        //todo: Rules for clearing matches;
        var matchFromLookup =
            request.Peid.Response.Matches.FirstOrDefault(x => x.Peid == request.Match.Peid);

        var relative = matchFromLookup?
            .Associates?
            .Where(x => CloudcheckRelationshipConsts.Child.Equals(x.Relationship, StringComparison.InvariantCultureIgnoreCase) || CloudcheckRelationshipConsts.Child.Equals(x.Relationship, StringComparison.InvariantCultureIgnoreCase))
            .ToList();

        if (relative == null || !relative.Any()) return Result.Success();

        if (_reviewMatchSettings.ClearEnabled)
        {
            var CloudcheckRequest = new ReviewMatchRequest
            {
                AssociationReference = request.Association.AssociationReference,
                Review = new Review
                {
                    Decision = "",
                    MatchId = request.Match.MatchId,
                    Notes = ""
                }
            };

            var response = await _client.ReviewMatchAsync(CloudcheckRequest, _CloudcheckSettings.ApiKey,
                _CloudcheckSettings.ApiSecret);
        }

        var log = new ReviewMatchLogEntity(request.Association.AssociationReference, request.Match.MatchId, "",
            _reviewMatchSettings.ClearEnabled);

        await _tableStorageService.InsertAsync(log, cancellationToken);

        return Result.Success();
    }
}