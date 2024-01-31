using Si.IdCheck.ApiClients.Verifidentity.Helpers;

namespace Si.IdCheck.ApiClients.Verifidentity.Models.Requests.PeidLookup;

public class PeidLookupRequest
{
    public int Peid { get; set; }
}

public static class PeidLookupRequestExtensions
{
    public static FormUrlEncodedContent BuildFormUrlEncodedContent(this PeidLookupRequest request, string path, string apiKey, string apiSecret)
    {
        var peidString = request.Peid.ToString();

        var nonce = VerifidentityHelpers.CreateNonce();

        var timestamp = VerifidentityHelpers.CreateUnixTimestamp();

        var parameters = new SortedDictionary<string, string>
        {
            ["key"] = apiKey,
            ["nonce"] = nonce,
            ["timestamp"] = timestamp,
            ["peid"] = peidString
        };

        var signature = VerifidentityHelpers.CreateSignature(parameters, path, apiSecret);

        var formData = new List<KeyValuePair<string, string>>
        {
            new ("key", apiKey),
            new ("signature", signature),
            new ("nonce", nonce),
            new ("timestamp", timestamp),
            new ("peid", peidString)
        };

        return new FormUrlEncodedContent(formData);
    }
}
    
