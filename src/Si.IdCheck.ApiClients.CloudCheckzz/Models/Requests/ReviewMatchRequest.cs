using System.Text.Json;
using System.Text.Json.Serialization;

namespace Si.IdCheck.ApiClients.CloudCheckzz.Models.Requests;

public class ReviewMatchRequest : IParameterBuilder, IPostRequestBuilder
{
    public string AssociationReference { get; set; }
    public Review Review { get; set; }

    private JsonSerializerOptions GetJsonSerializerOptions()
    {
        return new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    private string SerializeToJson()
    {
        var options = GetJsonSerializerOptions();
        return JsonSerializer.Serialize(this, options);
    }

    public SortedDictionary<string, string> BuildParameter(SortedDictionary<string, string> baseDictionary)
    {
        var data = SerializeToJson();

        baseDictionary.Add("data", data);

        return baseDictionary;
    }

    public CloudCheckPostRequestBase BuildPostRequest(string key, string nonce, string signature, string timestamp)
    {
        var data = SerializeToJson();

        return new CloudCheckRequest
        {
            Data = data,
            Key = key,
            Nonce = nonce,
            Signature = signature,
            TimeStamp = timestamp
        };
    }
}

public class Review
{
    /// <summary>
    /// The matchId is the unique identifier for a match within CloudCheck.
    /// </summary>
    public string MatchId { get; set; }
    public string Decision { get; set; }
    public string Notes { get; set; }
}
