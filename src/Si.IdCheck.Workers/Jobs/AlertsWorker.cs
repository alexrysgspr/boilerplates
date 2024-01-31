using Microsoft.Extensions.Options;
using Serilog;
using Si.IdCheck.Workers.Jobs.CronJob;
using Si.IdCheck.Workers.Services;
using Si.IdCheck.Workers.Settings;
namespace Si.IdCheck.Workers.Jobs;

public class AlertsWorker : CronJobWorker
{
    private readonly IAlertsWorkerService _alertsWorkerService;
    public AlertsWorker(IDateTimeService dateTimeService, 
        IOptions<AlertsWorkerSettings> cronWorkerSettings,
        IAlertsWorkerService alertsWorkerService) : base(dateTimeService, cronWorkerSettings.Value, Log.ForContext<AlertsWorker>())
    {
        _alertsWorkerService = alertsWorkerService;
    }

    public override async Task DoWorkAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _alertsWorkerService.DoWorkAsync(cancellationToken);
        }
        catch (Exception e)
        {
            Logger.Error(e, "An error occurred while running AlertsWorker");
        }
    }
}
