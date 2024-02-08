using MediatR;
using Si.IdCheck.Workers.Application.Models.Requests;

namespace Si.IdCheck.Workers.Services;

public interface IOngoingMonitoringAlertsService
{
    Task DoWorkAsync(string associationReference, CancellationToken cancellationToken);
}

public class OngoingMonitoringAlertsService : IOngoingMonitoringAlertsService
{
    private readonly IMediator _mediator;

    public OngoingMonitoringAlertsService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task DoWorkAsync(string associationReference, CancellationToken cancellationToken)
    {
        var associationRequest = new GetAssociation
        {
            AssociationReference = associationReference
        };

        //Get association details

        var associationResult = await _mediator.Send(associationRequest, cancellationToken);

        foreach (var match in associationResult.Value.Matches)
        {
            var getPersonDetailsRequest = new GetPersonDetails
            {
                Peid = match.Peid
            };

            //Get association's match details in lookup

            var personDetailsResult =
                await _mediator.Send(getPersonDetailsRequest, cancellationToken);

            var matches = personDetailsResult
                .Value
                .Response
                .Matches
                .Where(x => x != null)
                .ToList();

            foreach (var matchPersonDetails in matches)
            {
                //Get details of the match's associate
                var matchAssociatesDetailsRequest = new GetMatchAssociatesPersonDetailsRequest
                {
                    Associates = matchPersonDetails.Associates
                };
                var matchAssociatesPersonDetailsResult =
                    await _mediator.Send(matchAssociatesDetailsRequest, cancellationToken);

                var reviewMatchRequest = new ReviewMatch
                {
                    PersonOfInterest = associationResult.Value,
                    Match = match,
                    MatchAssociates = matchAssociatesPersonDetailsResult.Value,
                    MatchDetails = matchPersonDetails
                };

                await _mediator.Send(reviewMatchRequest, cancellationToken);
            }
        }
    }
}
