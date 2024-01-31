using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Options;
using Si.IdCheck.ApiClients.Verifidentity;
using Si.IdCheck.ApiClients.Verifidentity.Models.Requests;
using Si.IdCheck.ApiClients.Verifidentity.Models.Responses;
using Si.IdCheck.Workers.Application.Models.Requests;

namespace Si.IdCheck.Workers.Application.Handlers;
public class GetAssociationsHandler : IRequestHandler<GetAssociations, Result<List<Association>>>
{
    private readonly IVerifidentityApiClient _client;
    private readonly VerifidentitySettings _verifidentitySettings;

    public GetAssociationsHandler(
        IVerifidentityApiClient client,
        IOptions<VerifidentitySettings> verifidentitySettingsOption)
    {
        _client = client;
        _verifidentitySettings = verifidentitySettingsOption.Value;
    }

    public async Task<Result<List<Association>>> Handle(GetAssociations request, CancellationToken cancellationToken)
    {
        var verifidentityRequest = new GetAssociationsRequest
        {
            Cursor = 0,
            FilterAlertOnly = true,
            PageSize = 1000
        };

        var isLastPage = false;

        var associations = new List<Association>();

        while (!isLastPage)
        {
            var response = await _client.GetAssociationsAsync(verifidentityRequest, _verifidentitySettings.ApiKey, _verifidentitySettings.ApiSecret);

            associations.AddRange(response.Associations);

            if (int.TryParse(response.Meta.NextCursor, out var next))
            {
                verifidentityRequest.Cursor = next;
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
