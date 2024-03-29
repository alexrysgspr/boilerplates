﻿using System.Text.Json;
using Azure.Messaging.ServiceBus;
using MediatR;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Options;
using Serilog;
using Boilerplate.Workers.Application.ServiceBus;
using Boilerplate.Workers.Application.Settings;
using ILogger = Serilog.ILogger;

namespace Boilerplate.Workers.Jobs;

public class JobsWorker : BackgroundService
{

    private static readonly ILogger Logger = Log.ForContext<JobsWorker>();
    private ServiceBusProcessor _processor;
    private readonly IAzureClientFactory<ServiceBusClient> _azureClientFactory;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private CancellationToken _cancellationToken;
    private readonly ServiceBusSettings _serviceBusSettings;

    public JobsWorker(
        IAzureClientFactory<ServiceBusClient> azureClientFactory,
        IServiceScopeFactory serviceScopeFactory,
        IOptions<ServiceBusSettings> serviceBusSettingsOptions)
    {
        _azureClientFactory = azureClientFactory;
        _serviceScopeFactory = serviceScopeFactory;
        _serviceBusSettings = serviceBusSettingsOptions.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _cancellationToken = stoppingToken;
        _processor = _azureClientFactory
            .CreateClient(ServiceBusConsts.ClientName)
            .CreateProcessor(_serviceBusSettings.OngoingMonitoringAlertsQueueName, new ServiceBusProcessorOptions
            {
                MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(5),
                MaxConcurrentCalls = 1,
                AutoCompleteMessages = true
            });

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
            var subject = message.Subject;

            using var scope = _serviceScopeFactory.CreateScope();
            {
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            }
        }
        catch (Exception e)
        {
            await processMessageEvent.DeadLetterMessageAsync(message, cancellationToken: _cancellationToken);
            Logger.Error(e, $"An error occurred while processing message {message.MessageId}. Message subject: '{message.Subject}'. Error: {e.Message}");
        }
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs errorEvent)
    {
        Logger.Error(errorEvent.Exception, $"An error occurred while processing message in service bus queue. Error event: {JsonSerializer.Serialize(errorEvent)}");
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        Logger.Error(new Exception("JobsWorker stopped."), "JobsWorker stopped.");
        await _processor.CloseAsync(cancellationToken);
    }
}