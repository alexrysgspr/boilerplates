using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Options;
using Si.IdCheck.ApiClients.Cloudcheck;
using Si.IdCheck.ApiClients.Cloudcheck.Models.Requests;
using Si.IdCheck.ApiClients.Cloudcheck.Models.Responses;
using Si.IdCheck.Workers.Application.Models.Requests;
using Si.IdCheck.Workers.Application.Settings;

namespace Si.IdCheck.Workers.Application.Handlers;
public class GetAssociationsHandler : IRequestHandler<GetAssociations, Result<List<Association>>>
{
    private readonly ICloudcheckApiClient _client;
    private readonly CloudcheckSettings _CloudcheckSettings;
    private readonly GetAssociationsSettings _getAssociationsSettings;

    public GetAssociationsHandler(
        ICloudcheckApiClient client,
        IOptions<CloudcheckSettings> CloudcheckSettingsOption,
        IOptions<GetAssociationsSettings> getAssociationsSettingsOption)
    {
        _client = client;
        _CloudcheckSettings = CloudcheckSettingsOption.Value;
        _getAssociationsSettings = getAssociationsSettingsOption.Value;
    }

    public async Task<Result<List<Association>>> Handle(GetAssociations request, CancellationToken cancellationToken)
    {
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
            var response = await _client.GetAssociationsAsync(cloudCheckRequest, _CloudcheckSettings.ApiKey, _CloudcheckSettings.ApiSecret);

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

        //todo: Return associations here
        //todo: Client based credentials
        return Result.Success(associations);
    }
}
