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

    public override async Task DoWorkAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetService<IMediator>();

            var getAssociationsRequest = new GetAssociations();

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
                    var lookupPeidRequest = new LookupPeid
                    {
                        Peid = match.Peid
                    };

                    var lookupPeidResult = await mediator.Send(lookupPeidRequest, cancellationToken);

                    var reviewMatchRequest = new ReviewMatch
                    {
                        Association = associationResult.Value,
                        Peid = lookupPeidResult,
                        Match = match
                    };

                    await mediator.Send(reviewMatchRequest, cancellationToken);
                }
            }

        }
        catch (Exception e)
        {
            Logger.Error(e, "An error occurred while running AlertsWorker");
        }
    }
}
