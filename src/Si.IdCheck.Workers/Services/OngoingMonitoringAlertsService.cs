using MediatR;
using Si.IdCheck.ApiClients.CloudCheck.Models.Responses;
using Si.IdCheck.Workers.Application.Models.Requests;
using Si.IdCheck.Workers.Helpers;

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

        var concurrentWrites = 100;
        var pageCount = PagingHelpers.GetPageCount(associationResult.Value.Matches.Count, concurrentWrites);

        for (var i = 0; i < pageCount; i++)
        {
            var tasks = associationResult.Value.Matches
                .Skip(i * concurrentWrites)
                .Take(concurrentWrites)
                .Select(match => ReviewMatch(associationResult.Value, match, cancellationToken))
                .ToList();

            await Task.WhenAll(tasks);
        }
    }

    public async Task ReviewMatch(GetAssociationResponse association, Match match, CancellationToken cancellationToken)
    {
        var getPersonDetailsRequest = new GetPersonDetails
        {
            Peid = match.Peid
        };

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
                PersonOfInterest = association,
                Match = match,
                MatchAssociates = matchAssociatesPersonDetailsResult.Value,
                MatchDetails = matchPersonDetails
            };

            await _mediator.Send(reviewMatchRequest, cancellationToken);
        }
    }
}
