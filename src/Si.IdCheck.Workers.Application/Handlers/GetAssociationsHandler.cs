using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Options;
using Si.IdCheck.ApiClients.CloudCheck;
using Si.IdCheck.ApiClients.CloudCheck.Models.Requests;
using Si.IdCheck.ApiClients.CloudCheck.Models.Responses;
using Si.IdCheck.Workers.Application.Models.Requests;
using Si.IdCheck.Workers.Application.Settings;

namespace Si.IdCheck.Workers.Application.Handlers;
public class GetAssociationsHandler : IRequestHandler<GetAssociations, Result<List<Association>>>
{
    private readonly ICloudCheckApiClient _client;
    private readonly CloudCheckSettings _cloudCheckSettings;
    private readonly GetAssociationsSettings _getAssociationsSettings;

    public GetAssociationsHandler(
        ICloudCheckApiClient client,
        IOptions<CloudCheckSettings> CloudCheckSettingsOption,
        IOptions<GetAssociationsSettings> getAssociationsSettingsOption)
    {
        _client = client;
        _cloudCheckSettings = CloudCheckSettingsOption.Value;
        _getAssociationsSettings = getAssociationsSettingsOption.Value;
    }

    public async Task<Result<List<Association>>> Handle(GetAssociations request, CancellationToken cancellationToken)
    {
        var CloudCheckRequest = new GetAssociationsRequest
        {
            Cursor = 0,
            FilterAlertOnly = _getAssociationsSettings.FilterAlertOnly,
            PageSize = _getAssociationsSettings.PageSize,
        };

        var isLastPage = false;

        var associations = new List<Association>();

        while (!isLastPage)
        {
            var response = await _client.GetAssociationsAsync(CloudCheckRequest, _cloudCheckSettings.ApiKey, _cloudCheckSettings.ApiSecret);

            associations.AddRange(response.Associations);

            if (int.TryParse(response.Meta.NextCursor, out var next))
            {
                CloudCheckRequest.Cursor = next;
            }
            else
            {
                isLastPage = true;
            }
        }

        //todo: Return associations here
        //todo: Client based credentials
        return Result.Success(associations);
    }
}
