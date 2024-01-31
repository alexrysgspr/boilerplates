using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Serilog;
using Si.IdCheck.ServiceBus;
using ILogger = Serilog.ILogger;

namespace Si.IdCheck.Workers.Jobs;

public class JobsWorker : BackgroundService
{

    private CancellationToken _cancellationToken;
    private static readonly ILogger Logger = Log.ForContext<JobsWorker>();
    private readonly IServiceBusFactory _serviceBusFactory;
    private ServiceBusProcessor _processor;

    public JobsWorker(IServiceBusFactory serviceBusFactory)
    {
        _serviceBusFactory = serviceBusFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _cancellationToken = stoppingToken;

        //todo:
        _processor = _serviceBusFactory.CreateProcessor("");

        _processor.ProcessMessageAsync += ProcessMessageAsync;
        _processor.ProcessErrorAsync += ProcessErrorAsync;

        do
        {
            if (!_processor.IsProcessing)
            {
                await _processor.StartProcessingAsync(stoppingToken);
            }

            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);

        } while (!stoppingToken.IsCancellationRequested);
    }

    private async Task ProcessMessageAsync(ProcessMessageEventArgs processMessageEvent)
    {
        var message = processMessageEvent.Message;

        try
        {
            switch (message.Subject)
            {

            }
        }
        catch (Exception e)
        {
            Logger.Error(e, $"An unexpected error occurred while processing message {message.MessageId}. Message subject: '{message.Subject}'.");
        }
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs errorEvent)
    {
        Logger.Error(errorEvent.Exception, $"An unexpected error occurred while processing message in service bus queue. Error event: {JsonSerializer.Serialize(errorEvent)}");
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        Logger.Error(new Exception("JobsWorker stopped."), "JobsWorker stopped.");
        await _processor.CloseAsync(cancellationToken);
    }
}