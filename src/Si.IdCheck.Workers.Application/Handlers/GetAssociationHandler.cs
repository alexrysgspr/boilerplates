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
public class GetAssociationHandler : IRequestHandler<GetAssociation, Result<GetAssociationResponse>>
{
    private readonly ICloudCheckApiClient _client;
    private readonly IOptionsFactory<ReviewerSettings> _settingsFactory;
    private static readonly ILogger Logger = Log.ForContext<GetAssociationHandler>();
    private readonly IAzureClientFactory<ServiceBusClient> _azureClientFactory;
    private readonly ServiceBusSettings _serviceBusSettings;

    public GetAssociationHandler(
        ICloudCheckApiClient client,
        IOptionsFactory<ReviewerSettings> settingsFactory,
        IAzureClientFactory<ServiceBusClient> azureClientFactory,
        IOptions<ServiceBusSettings> serviceBusSettingsOptions)
    {
        _settingsFactory = settingsFactory;
        _client = client;
        _azureClientFactory = azureClientFactory;
        _serviceBusSettings = serviceBusSettingsOptions.Value;
    }

    public async Task<Result<GetAssociationResponse>> Handle(GetAssociation request, CancellationToken cancellationToken)
    {
        var validator = new GetAssociationValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            return Result.Invalid(validationResult.AsErrors());
        }

        var settings = _settingsFactory.Create(request.ClientId);
        var cloudCheckRequest = new GetAssociationRequest
        {
            AssociationReference = request.AssociationReference
        };

        var association = await _client.GetAssociationAsync(cloudCheckRequest, settings.ApiKey,
            settings.ApiSecret);

        association.Matches = association
            .Matches
            .Where(match => settings.RelationshipTypes.Contains(match.Type, StringComparer.InvariantCultureIgnoreCase) && settings.RiskTypes.Any(y => match.RiskTypes.Select(rt => rt.Code).Contains(y, StringComparer.InvariantCultureIgnoreCase)))
            .ToList();

        if (association.Matches is null or { Count: 0 })
        {
            return Result.Success(association);
        }

        Logger.Information($"Processing association with id '{request.AssociationReference}', match count: {association.Matches.Count}.");

        var concurrentWrites = 100;
        var pageCount = PagingHelpers.GetPageCount(association.Matches.Count, concurrentWrites);

        var serviceBusClient = _azureClientFactory
            .CreateClient(ServiceBusConsts.ClientName)
            .CreateSender(_serviceBusSettings.OngoingMonitoringAlertsQueueName);

        for (var i = 0; i < pageCount; i++)
        {
            var tasks = association.Matches
                .Skip(i * concurrentWrites)
                .Take(concurrentWrites)
                .Select(x => serviceBusClient.SendMessageAsync(
                    ServiceBusHelpers
                    .CreateMessage(association
                            .ToServiceBusMessage(x, request.ClientId), ServiceBusConsts.OngoingMonitoringAlerts.MessageTypes.ReviewMatch), cancellationToken))
                .ToList();

            await Task.WhenAll(tasks);
        }

        return Result.Success(association);
    }
}
