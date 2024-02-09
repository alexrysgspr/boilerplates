using System.Text;
using Azure.Messaging.ServiceBus;
using MediatR;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Options;
using Serilog;
using Si.IdCheck.Workers.Application.Models.Requests;
using Si.IdCheck.Workers.Constants;
using Si.IdCheck.Workers.Helpers;
using Si.IdCheck.Workers.Jobs.CronJob;
using Si.IdCheck.Workers.Services;
using Si.IdCheck.Workers.Settings;
namespace Si.IdCheck.Workers.Jobs;

public class OngoingMonitoringAlertsWorker : CronJobWorker
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ServiceBusSender _serviceBusClient;

    public OngoingMonitoringAlertsWorker(IDateTimeService dateTimeService,
        IOptions<Settings.OngoingMonitoringAlertsWorkerSettings> cronWorkerSettings,
        IServiceScopeFactory serviceScopeFactory,
        IAzureClientFactory<ServiceBusClient> azureClientFactory,
        IOptions<ServiceBusSettings> serviceBusSettingsOptions) : base(dateTimeService, cronWorkerSettings.Value, Log.ForContext<OngoingMonitoringAlertsWorker>())
    {
        _serviceScopeFactory = serviceScopeFactory;
        _serviceBusClient = azureClientFactory.CreateClient(ServiceBusConsts.ClientName).CreateSender(serviceBusSettingsOptions.Value.OngoingMonitoringAlertsQueueName);
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

            var concurrentWrites = 100;
            var pageCount = PagingHelpers.GetPageCount(associations.Value.Count, concurrentWrites);

            for (var i = 0; i < pageCount; i++)
            {
                var tasks = associations
                    .Value
                        .Skip(i * concurrentWrites)
                    .Take(concurrentWrites)
                    .Select(x => _serviceBusClient.SendMessageAsync(new ServiceBusMessage
                    {
                        Body = new BinaryData(Encoding.UTF8.GetBytes(x.AssociationReference))
                    }, cancellationToken));

                try
                {
                    await Task.WhenAll(tasks);
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Failed writing / publishing portfolio cash details.");
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
