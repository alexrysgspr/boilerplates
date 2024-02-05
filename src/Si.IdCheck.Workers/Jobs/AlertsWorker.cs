using MediatR;
using Microsoft.Extensions.Options;
using Serilog;
using Si.IdCheck.Workers.Application.Models.Requests;
using Si.IdCheck.Workers.Jobs.CronJob;
using Si.IdCheck.Workers.Services;
using Si.IdCheck.Workers.Settings;
namespace Si.IdCheck.Workers.Jobs;

public class AlertsWorker : CronJobWorker
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public AlertsWorker(IDateTimeService dateTimeService, 
        IOptions<AlertsWorkerSettings> cronWorkerSettings,
        IServiceScopeFactory serviceScopeFactory) : base(dateTimeService, cronWorkerSettings.Value, Log.ForContext<AlertsWorker>())
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    private bool isRunning;


    public override async Task DoWorkAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (isRunning)
                return;

            using var scope = _serviceScopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetService<IMediator>();

            var getAssociationsRequest = new GetAssociations();
            isRunning = true;

            //Get associations
            var associations = await mediator.Send(getAssociationsRequest, cancellationToken);

            foreach (var association in associations.Value)
            {
                var associationRequest = new GetAssociation
                {
                    AssociationReference = association.AssociationReference
                };

                //Get association details
                var associationResult = await mediator.Send(associationRequest, cancellationToken);

                foreach (var match in associationResult.Value.Matches)
                {
                    var getPersonDetailsRequest = new GetPersonDetails
                    {
                        Peid = match.Peid
                    };

                    //Get association's match details in lookup
                    var personDetailsResult = await mediator.Send(getPersonDetailsRequest, cancellationToken);

                    foreach (var matchPersonDetails in personDetailsResult.Value.Response.Matches)
                    {
                        //Get details of the match's associate
                        var matchAssociatesDetailsRequest = new GetMatchAssociatesPersonDetailsRequest
                        {
                            Associates = matchPersonDetails.Associates
                        };

                        var matchAssociatesPersonDetailsResult = await mediator.Send(matchAssociatesDetailsRequest, cancellationToken);

                        var reviewMatchRequest = new ReviewMatch
                        {
                            PersonOfInterest = associationResult.Value,
                            Match = match,
                            MatchAssociates = matchAssociatesPersonDetailsResult,
                            MatchDetails = matchPersonDetails
                        };

                        await mediator.Send(reviewMatchRequest, cancellationToken);
                    }
                }
            }

        }
        catch (Exception e)
        {
            await Task.Delay(3000, cancellationToken);
            isRunning = false;
            Logger.Error(e, "An error occurred while running AlertsWorker");
        }
    }
}
