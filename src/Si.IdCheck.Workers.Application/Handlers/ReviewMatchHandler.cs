using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Options;
using Si.IdCheck.ApiClients.Verifidentity;
using Si.IdCheck.ApiClients.Verifidentity.Models.Requests;
using Si.IdCheck.Workers.Application.Models.Requests;
using Si.IdCheck.Workers.Application.Settings;

namespace Si.IdCheck.Workers.Application.Handlers;
public class ReviewMatchHandler : IRequestHandler<ReviewMatch, Result>
{
    private readonly IVerifidentityApiClient _client;
    private readonly VerifidentitySettings _verifidentitySettings;
    private readonly ReviewMatchSettings _reviewMatchSettings;

    public ReviewMatchHandler(
        IVerifidentityApiClient client,
        IOptions<VerifidentitySettings> verifidentitySettingsOption,
        IOptions<ReviewMatchSettings> reviewMatchSettingsOptions)
    {
        _client = client;
        _verifidentitySettings = verifidentitySettingsOption.Value;
        _reviewMatchSettings = reviewMatchSettingsOptions.Value;
    }

    public async Task<Result> Handle(ReviewMatch request, CancellationToken cancellationToken)
    {
        //todo: Rules for clearing matches;
        {
            if (_reviewMatchSettings.ClearEnabled)
            {
                var verifidentityRequest = new ReviewMatchRequest
                {
                    AssociationReference = request.Association.AssociationReference,
                    Review = new Review
                    {
                        Decision = "",
                        MatchId = request.Match.MatchId,
                        Notes = ""
                    }
                };

                var response = await _client.ReviewMatchAsync(verifidentityRequest, _verifidentitySettings.ApiKey,
                    _verifidentitySettings.ApiSecret);
            }

            //todo: Add log to why it's been cleared
        }

        return Result.Success();
    }
}