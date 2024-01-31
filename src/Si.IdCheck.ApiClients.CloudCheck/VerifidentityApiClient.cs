using System.Text.Json;
using Serilog;
using Si.IdCheck.ApiClients.Verifidentity.Helpers;
using Si.IdCheck.ApiClients.Verifidentity.Models.Requests;
using Si.IdCheck.ApiClients.Verifidentity.Models.Responses;

namespace Si.IdCheck.ApiClients.Verifidentity;

public interface IVerifidentityApiClient
{
    public Task<GetAssociationResponse> GetAssociationAsync(GetAssociationRequest request, string apiKey, string apiSecret);
    public Task<GetAssociationsResponse> GetAssociationsAsync(GetAssociationsRequest request, string apiKey, string apiSecret);
    Task<ReviewMatchResponse> GetReviewMatchAsync(ReviewMatchRequest request, string apiKey, string apiSecret);
}

public class VerifidentityApiClient : IVerifidentityApiClient
{
    private readonly HttpClient _client;
    private static readonly ILogger Logger = Log.ForContext<VerifidentityApiClient>();
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    public VerifidentityApiClient(HttpClient client)
    {
        _client = client;
    }
    public async Task<GetAssociationResponse> GetAssociationAsync(GetAssociationRequest request, string apiKey, string apiSecret)
    {
        var path = "/watchlist/association/";
        var queryParams = request.ToQueryParams(path, apiKey, apiSecret);
        try
        {
            var responseMessage = await _client.GetAsync($"{path}{queryParams}");

            if (responseMessage.IsSuccessStatusCode)
            {
                await HandleVerifyFailureResponse(responseMessage);

                var responseString = await responseMessage.Content.ReadAsStringAsync();
                var response = JsonSerializer.Deserialize<GetAssociationResponse>(responseString, JsonOptions);

                return response;
            }

            throw new Exception();
        }
        catch (Exception e)
        {
            throw;
        }
    }

    public async Task<GetAssociationsResponse> GetAssociationsAsync(GetAssociationsRequest request, string apiKey, string apiSecret)
    {
        var path = "/watchlist/associations/";
        var queryParams = request.ToQueryParams(path, apiKey, apiSecret);

        try
        {
            var responseMessage = await _client.GetAsync($"{path}{queryParams}");

            if (responseMessage.IsSuccessStatusCode)
            {
                await HandleVerifyFailureResponse(responseMessage);
                var responseString = await responseMessage.Content.ReadAsStringAsync();
                var response = JsonSerializer.Deserialize<GetAssociationsResponse>(responseString, JsonOptions);

                return response;
            }

            throw new Exception();
        }
        catch (Exception e)
        {
            throw;
        }
    }

    public async Task<ReviewMatchResponse> GetReviewMatchAsync(ReviewMatchRequest request, string apiKey, string apiSecret)
    {
        var path = "/watchlist/review-match/";
        var pairs = VerifidentityHelpers
            .CreatePostRequest(request, path, apiKey, apiSecret)
            .ToDictionary()
            .ToCamelCaseKeys();

        var content = new FormUrlEncodedContent(pairs);
        var httpRequestMessage =
            new HttpRequestMessage(HttpMethod.Post, path)
            {
                Content = content
            };

        try
        {
            var response = await _client.SendAsync(httpRequestMessage);

            var responseString = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                await HandleVerifyFailureResponse(response);

                var idCheckResponse = JsonSerializer.Deserialize<ReviewMatchResponse>(responseString, JsonOptions);

                return idCheckResponse;
            }

            var exception = new Exception("Failed to send Verifidentity request.");
            Logger.Error(exception, $"Failed to send Verifidentity request. Error: {responseString}. StatusCode: {response.StatusCode}.");
            throw exception;
        }
        catch (Exception e)
        {
            Logger.Error(e, "Failed to send Verifidentity request.");
            throw;
        }
    }

    private async Task HandleVerifyFailureResponse(HttpResponseMessage httpResponseMessage)
    {
        var responseString = await httpResponseMessage.Content.ReadAsStringAsync();
        var response =
            JsonSerializer.Deserialize<VerifidentityResponse>(responseString, JsonOptions);

        if (response is { Verification.Error: { } } )
        {
            var error = new Exception($"Verifidentity check failed. Response error code: {response.Verification.Error.Value}. Message: '{response.Verification.Message}'.");
            Logger.Error(error, error.Message);
            throw error;
        }
    }
}
