using Microsoft.Extensions.Options;
using Serilog;
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

    private bool isRunning = true;


    public override async Task DoWorkAsync(CancellationToken cancellationToken)
    {
        
    }
}
