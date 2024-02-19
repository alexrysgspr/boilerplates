using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Serilog;

namespace Boilerplate.ApiClients.Boilerplate;

public interface IBoilerplateApiClient
{
    public Task<dynamic> GetAsync(dynamic request, CancellationToken cancellationToken = default);
}

public class BoilerplateApiClient(HttpClient client) : IBoilerplateApiClient
{
    private static readonly ILogger Logger = Log.ForContext<BoilerplateApiClient>();
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<dynamic> GetAsync(dynamic request, CancellationToken cancellationToken = default)
    {
        var path = "";
        try
        {
            var responseMessage = await client.GetAsync($"{path}", cancellationToken);

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseString = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
                var response = JsonSerializer.Deserialize<dynamic>(responseString, JsonOptions);

                return response;
            }

            var exception = new Exception($"Request failed Request: '{path}'. Response error code: {responseMessage.StatusCode}. Message: '{await responseMessage.Content.ReadAsStringAsync(cancellationToken)}'. Path: {path}.");
            Logger.Error(exception, "An error occurred while sending request.");

            throw exception;
        }
        catch (Exception e)
        {
            Logger.Error(e, $"An error occurred while sending request.");
            throw;
        }
    }

    public async Task<dynamic> PostAsync(dynamic request, CancellationToken cancellationToken = default)
    {
        var content = new StringContent("", Encoding.UTF8, new MediaTypeHeaderValue("application/json"));
        try
        {
            var response = await client.PostAsync("", content, cancellationToken);

            var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var idCheckResponse = JsonSerializer.Deserialize<dynamic>(responseString, JsonOptions);

                return idCheckResponse;
            }

            var exception = new Exception($"Request failed. Response error code: {response.StatusCode}. Message: '{await response.Content.ReadAsStringAsync(cancellationToken)}'.");
            Logger.Error(exception, "An error occurred while sending request");
            throw exception;
        }
        catch (Exception e)
        {
            Logger.Error(e, $"An error occurred while sending request.");
            throw;
        }
    }
}
