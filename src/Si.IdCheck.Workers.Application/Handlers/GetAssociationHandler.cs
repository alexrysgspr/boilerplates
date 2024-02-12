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
    private readonly IOptionsSnapshot<ReviewerSettings> _settingsFactory;
    private readonly ServiceBusSender _serviceBusClient;
    private static readonly ILogger Logger = Log.ForContext<GetAssociationHandler>();
    public GetAssociationHandler(
        ICloudCheckApiClient client,
        IOptionsSnapshot<ReviewerSettings> settingsFactory,
        IAzureClientFactory<ServiceBusClient> azureClientFactory,
        IOptions<ServiceBusSettings> serviceBusSettingsOptions)
    {
        _settingsFactory = settingsFactory;
        _client = client;
        _serviceBusClient = azureClientFactory
            .CreateClient(ServiceBusConsts.ClientName)
            .CreateSender(serviceBusSettingsOptions.Value.OngoingMonitoringAlertsQueueName);
    }

    public async Task<Result<GetAssociationResponse>> Handle(GetAssociation request, CancellationToken cancellationToken)
    {
        var validator = new GetAssociationValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            return Result.Invalid(validationResult.AsErrors());
        }

        var settings = _settingsFactory.Get(request.ClientId);
        var cloudCheckRequest = new GetAssociationRequest
        {
            AssociationReference = request.AssociationReference
        };

        var association = await _client.GetAssociationAsync(cloudCheckRequest, settings.ApiKey,
            settings.ApiSecret);

        association.Matches = association
            .Matches
            .Where(match => settings.RelationshipTypes.Contains(match.Type, StringComparer.InvariantCultureIgnoreCase) && !settings.RiskTypes.All(y => match.RiskTypes.Select(rt => rt.Code).Contains(y, StringComparer.InvariantCultureIgnoreCase)))
            .ToList();

        Logger.Information($"Processing association with id '{request.AssociationReference}', match count: {association.Matches.Count}.");

        var concurrentWrites = 100;
        var pageCount = PagingHelpers.GetPageCount(association.Matches.Count, concurrentWrites);

        for (var i = 0; i < pageCount; i++)
        {
            var tasks = association.Matches
                .Skip(i * concurrentWrites)
                .Take(concurrentWrites)
                .Select(x => _serviceBusClient.SendMessageAsync(ServiceBusHelpers.CreateMessage(
                        new OngoingMonitoringAlertMessages.ReviewMatch
                        {
                            AssociationReference = association.AssociationReference,
                            Peid = x.Peid,
                            ClientId = request.ClientId,
                            MatchId = x.MatchId,
                            PersonOfInterestBirthYear = association.PersonDetail?.BirthYear
                        }, ServiceBusConsts.OngoingMonitoringAlerts.MessageTypes.ReviewMatch), cancellationToken))
                .ToList();

            await Task.WhenAll(tasks);
        }

        return Result.Success();
    }
}
