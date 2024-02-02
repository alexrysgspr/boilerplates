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

            var associations = await mediator.Send(getAssociationsRequest, cancellationToken);
            foreach (var association in associations.Value)
            {
                var associationRequest = new GetAssociation
                {
                    AssociationReference = association.AssociationReference
                };

                var associationResult = await mediator.Send(associationRequest, cancellationToken);


                foreach (var match in associationResult.Value.Matches)
                {
                    var lookupPeidRequest = new GetPeidDetails
                    {
                        Peid = match.Peid
                    };

                    var matchPeidResult = await mediator.Send(lookupPeidRequest, cancellationToken);

                    foreach (var matchPeid in matchPeidResult.Value.Response.Matches)
                    {
                        var matchAssociatesPeidDetailsRequest = new GetMatchAssociatesPeidDetailsRequest
                        {
                            Associates = matchPeid.Associates
                        };

                        var associatePeidDetailsResult = await mediator.Send(matchAssociatesPeidDetailsRequest, cancellationToken);

                        var reviewMatchRequest = new ReviewMatch
                        {
                            AssociationReference = association.AssociationReference,
                            Associates = associatePeidDetailsResult,
                            Match = match,
                            MatchDetails = matchPeid
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
