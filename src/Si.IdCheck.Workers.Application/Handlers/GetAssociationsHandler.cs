using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Options;
using Si.IdCheck.ApiClients.CloudCheck;
using Si.IdCheck.ApiClients.CloudCheck.Models.Requests;
using Si.IdCheck.ApiClients.CloudCheck.Models.Responses;
using Si.IdCheck.Workers.Application.Models.Requests;
using Si.IdCheck.Workers.Application.Reviewers;
using Si.IdCheck.Workers.Application.Settings;

namespace Si.IdCheck.Workers.Application.Handlers;
public class GetAssociationsHandler : IRequestHandler<GetAssociations, Result<List<Association>>>
{
    private readonly ICloudCheckApiClient _client;
    private readonly GetAssociationsSettings _getAssociationsSettings;
    private readonly IOptionsFactory<ReviewerSettings> _settingsFactory;

    public GetAssociationsHandler(
        ICloudCheckApiClient client,
        IOptions<GetAssociationsSettings> getAssociationsSettingsOption,
        IOptionsFactory<ReviewerSettings> settingsFactory)
    {
        _client = client;
        _getAssociationsSettings = getAssociationsSettingsOption.Value;
        _settingsFactory = settingsFactory;
    }

    public async Task<Result<List<Association>>> Handle(GetAssociations request, CancellationToken cancellationToken)
    {
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

        return Result.Success(associations);
    }
}
