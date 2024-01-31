using System;
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
    Task<ReviewMatchResponse> ReviewMatchAsync(ReviewMatchRequest request, string apiKey, string apiSecret);
    Task<PeidLookupResponse> LookupPeidAsync(PeidLookupRequest request, string apiKey, string apiSecret);
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

            var exception = new Exception($"Verifidentity check failed. Response error code: {responseMessage.StatusCode}. Message: '{await responseMessage.Content.ReadAsStringAsync()}'. Path: {path}.");
            Logger.Error(exception, "An error occurred while sending request for get association.");

            throw exception;
        }
        catch (Exception e)
        {
            Logger.Error(e, "An error occurred while sending request for get association.");
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

            var exception = new Exception($"Verifidentity check failed. Response error code: {responseMessage.StatusCode}. Message: '{await responseMessage.Content.ReadAsStringAsync()}'. Path: {path}.");
            Logger.Error(exception, "An error occurred while sending request for get associations.");

            throw exception;
        }
        catch (Exception e)
        {
            Logger.Error(e, "An error occurred while sending request for get associations.");
            throw;
        }
    }

    public async Task<ReviewMatchResponse> ReviewMatchAsync(ReviewMatchRequest request, string apiKey, string apiSecret)
    {
        var path = "/watchlist/review-match/";
        var pairs = VerifidentityHelpers
            .CreatePostRequest(request, path, apiKey, apiSecret)
            .ToDictionary()
            .ToLowerCaseKeys();

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

            var exception = new Exception($"Verifidentity check failed. Response error code: {response.StatusCode}. Message: '{await response.Content.ReadAsStringAsync()}'. Path: {path}.");
            Logger.Error(exception, "An error occurred while sending request for review match.");

            throw exception;
        }
        catch (Exception e)
        {
            Logger.Error(e, "An error occurred while sending request for review match..");
            throw;
        }
    }

    public async Task<PeidLookupResponse> LookupPeidAsync(PeidLookupRequest request, string apiKey, string apiSecret)
    {
        var path = "/verify/peid/";
        var pairs = VerifidentityHelpers
            .CreatePostRequest(request, path, apiKey, apiSecret)
            .ToDictionary()
            .ToLowerCaseKeys();

        var content = new FormUrlEncodedContent(pairs);

        //var content = request
        //    .BuildFormUrlEncodedContent(path, apiKey, apiSecret);

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

                var idCheckResponse = JsonSerializer.Deserialize<PeidLookupResponse>(responseString, JsonOptions);

                return idCheckResponse;
            }

            var exception = new Exception($"Verifidentity check failed. Response error code: {response.StatusCode}. Message: '{await response.Content.ReadAsStringAsync()}'. Path: {path}.");
            Logger.Error(exception, "An error occurred while sending request for lookup peid.");
            throw exception;
        }
        catch (Exception e)
        {
            Logger.Error(e, "An error occurred while sending request for lookup peid.");
            throw;
        }
    }

    private async Task HandleVerifyFailureResponse(HttpResponseMessage httpResponseMessage)
    {
        var responseString = await httpResponseMessage.Content.ReadAsStringAsync();
        var response =
            JsonSerializer.Deserialize<VerifidentityResponse>(responseString, JsonOptions);

        if (response is { Verification.Error: { } })
        {
            var error = new Exception($"Verifidentity check failed. Response error code: {response.Verification.Error.Value}. Message: '{response.Verification.Message}'.");
            Logger.Error(error, error.Message);
            throw error;
        }
    }
}
