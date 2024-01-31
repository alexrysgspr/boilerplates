using Microsoft.Extensions.Options;
using Si.IdCheck.ApiClients.Verifidentity;
using Si.IdCheck.ApiClients.Verifidentity.Models.Requests;
using Si.IdCheck.ApiClients.Verifidentity.Models.Responses;

namespace Si.IdCheck.Workers.Services;

public interface IAlertsWorkerService
{
    Task DoWorkAsync(CancellationToken cancellationToken);
}

public class AlertsWorkerService : IAlertsWorkerService
{
    private readonly IVerifidentityApiClient _client;
    private readonly VerifidentitySettings _verifidentitySettings;

    public AlertsWorkerService(
        IVerifidentityApiClient client,
        IOptions<VerifidentitySettings> verifidentitySettingsOptions)
    {
        _verifidentitySettings = verifidentitySettingsOptions.Value;
        _client = client;
    }

    public async Task DoWorkAsync(CancellationToken cancellationToken)
    {
        var request = new GetAssociationsRequest
        {
            Cursor = 0,
            FilterAlertOnly = true,
            PageSize = 1000
        };

        var associations = new List<Association>();

        var isLastPage = false;

        while (!isLastPage)
        {
            var response = await _client.GetAssociationsAsync(request, _verifidentitySettings.ApiKey, _verifidentitySettings.ApiSecret);
            associations.AddRange(response.Associations);

            if (int.TryParse(response.Meta.NextCursor, out var next))
            {
                request.Cursor = next;
            }
            else
            {
                isLastPage = true;
            }
        }

        await ClearAssociationsAsync(associations);
    }

    private async Task ClearAssociationsAsync(List<Association> associations)
    {
        throw new NotImplementedException();
    }
}
