using Si.IdCheck.ApiClients.Cloudcheck.Helpers;

namespace Si.IdCheck.ApiClients.Cloudcheck.Models.Requests;
public class GetAssociationRequest
{
    /// <summary>
    /// Unique Cloudcheck identifier for an association.
    /// </summary>
    public string AssociationReference { get; set; }
}

public static class GetAssociationsRequestExtensions
{
    public static string ToQueryParams(this GetAssociationRequest request, string path, string apiKey, string apiSecret)
    {
        var nonce = CloudcheckHelpers.CreateNonce();

        var timestamp = CloudcheckHelpers.CreateUnixTimestamp();

        var parameters = new SortedDictionary<string, string>
        {
            ["key"] = apiKey,
            ["nonce"] = nonce,
            ["timestamp"] = timestamp,
            ["associationReference"] = request.AssociationReference
        };

        var signature = CloudcheckHelpers.CreateSignature(parameters, path, apiSecret);

        return
            $"?key={apiKey}&nonce={nonce}&timestamp={timestamp}&signature={signature}&associationReference={request.AssociationReference}";
    }
}