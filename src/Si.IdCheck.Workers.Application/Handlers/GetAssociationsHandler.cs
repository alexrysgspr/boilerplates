using Ardalis.Result;
using Ardalis.Result.FluentValidation;
using Azure.Messaging.ServiceBus;
using MediatR;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Options;
using Serilog;
using Si.IdCheck.ApiClients.CloudCheck;
using Si.IdCheck.ApiClients.CloudCheck.Models.Requests;
using Si.IdCheck.ApiClients.CloudCheck.Models.Responses;
using Si.IdCheck.Workers.Application.Helpers;
using Si.IdCheck.Workers.Application.Models.Requests;
using Si.IdCheck.Workers.Application.Reviewers;
using Si.IdCheck.Workers.Application.ServiceBus;
using Si.IdCheck.Workers.Application.Settings;

namespace Si.IdCheck.Workers.Application.Handlers;
public class GetAssociationsHandler : IRequestHandler<GetAssociations, Result>
{
    private readonly ICloudCheckApiClient _client;
    private readonly GetAssociationsSettings _getAssociationsSettings;
    private readonly IOptionsFactory<ReviewerSettings> _settingsFactory;
    private readonly ServiceBusSender _serviceBusClient;
    private static readonly ILogger Logger = Log.ForContext<GetAssociationsHandler>();
    public GetAssociationsHandler(
        ICloudCheckApiClient client,
        IOptions<GetAssociationsSettings> getAssociationsSettingsOption,
        IOptionsFactory<ReviewerSettings> settingsFactory,
        IAzureClientFactory<ServiceBusClient> azureClientFactory,
        IOptions<ServiceBusSettings> serviceBusSettingsOptions)
    {
        _client = client;
        _getAssociationsSettings = getAssociationsSettingsOption.Value;
        _settingsFactory = settingsFactory;
        _serviceBusClient = azureClientFactory
            .CreateClient(ServiceBusConsts.ClientName)
            .CreateSender(serviceBusSettingsOptions.Value.OngoingMonitoringAlertsQueueName);
    }

    public async Task<Result> Handle(GetAssociations request, CancellationToken cancellationToken)
    {
        var validator = new GetAssociationsValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Invalid(validationResult.AsErrors());
        }

        var settings = _settingsFactory.Create(request.ClientId);

        var cloudCheckRequest = new GetAssociationsRequest
        {
            Cursor = 0,
            FilterAlertOnly = _getAssociationsSettings.FilterAlertOnly,
            PageSize = _getAssociationsSettings.PageSize,
        };

        var isLastPage = false;

        var associations = new List<Association>();

        while (!isLastPage)
        {
            var response = await _client.GetAssociationsAsync(cloudCheckRequest, settings.ApiKey, settings.ApiSecret);

            associations.AddRange(response.Associations);

            if (int.TryParse(response.Meta.NextCursor, out var next))
            {
                cloudCheckRequest.Cursor = next;
            }
            else
            {
                isLastPage = true;
            }
        }

        associations = associations
            .Where(x => settings.AssociationTypes.Contains(x.Type, StringComparer.InvariantCultureIgnoreCase))
            .ToList();

        Logger.Information( $"Processing associations: {associations.Count}.");

        var concurrentWrites = 100;
        var pageCount = PagingHelpers.GetPageCount(associations.Count, concurrentWrites);

        for (var i = 0; i < pageCount; i++)
        {
            var tasks = associations
                .Skip(i * concurrentWrites)
                .Take(concurrentWrites)
                .Select(x => _serviceBusClient.SendMessageAsync(ServiceBusHelpers.CreateMessage(
                    new OngoingMonitoringAlertMessages.GetAssociation
                    {
                        AssociationReference = x.AssociationReference,
                        ClientId = request.ClientId
                    }, ServiceBusConsts.OngoingMonitoringAlerts.MessageTypes.GetAssociation), cancellationToken))
                .ToList();
            
            await Task.WhenAll(tasks);
        }

        return Result.Success();
    }
}
