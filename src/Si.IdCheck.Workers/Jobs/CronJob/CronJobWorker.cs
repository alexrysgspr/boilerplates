using Cronos;
using Si.IdCheck.Workers.Services;
using Si.IdCheck.Workers.Settings;
using ILogger = Serilog.ILogger;
namespace Si.IdCheck.Workers.Jobs.CronJob
{
    public abstract class CronJobWorker : IHostedService, IDisposable
    {
        private System.Timers.Timer _timer;
        private readonly CronExpression _expression;
        protected readonly IDateTimeService DateTimeService;
        protected readonly CronWorkerSettings WorkerSettings;
        protected readonly ILogger Logger;

        protected CronJobWorker(
            IDateTimeService dateTimeService,
            CronWorkerSettings cronWorkerSettings,
            ILogger logger)
        {
            Logger = logger;
            DateTimeService = dateTimeService;
            WorkerSettings = cronWorkerSettings;
            _expression = CronExpression.Parse(cronWorkerSettings.Schedule, CronFormat.IncludeSeconds);
        }

        public virtual async Task StartAsync(CancellationToken cancellationToken)
        {
            await ScheduleJobAsync(cancellationToken);
        }

        private async Task ScheduleJobAsync(CancellationToken cancellationToken)
        {
            var next = _expression.GetNextOccurrence(DateTimeService.UtcDateTimeNow, DateTimeService.AetTimeZoneInfo);
            if (next.HasValue)
            {
                var delay = next.Value - DateTimeService.UtcDateTimeNow;
                Logger.Information($"Schedule: '{_expression}' next run in: '{delay}'");
                _timer = new System.Timers.Timer(delay.TotalMilliseconds);
                _timer.Elapsed += async (_, __) =>
                {
                    _timer.Dispose();
                    _timer = null;

                    if (!cancellationToken.IsCancellationRequested)
                    {
                        if (WorkerSettings.Enabled)
                        {
                            await DoWorkAsync(cancellationToken);
                        }
                        else
                        {
                            Logger.Information($"{GetType().Name} disabled.");
                        }
                    }

                    if (!cancellationToken.IsCancellationRequested)
                    {
                        await ScheduleJobAsync(cancellationToken);
                    }
                };
                _timer.Start();
            }
            await Task.CompletedTask;
        }

        public abstract Task DoWorkAsync(CancellationToken cancellationToken);

        public virtual async Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Stop();
            await Task.CompletedTask;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
