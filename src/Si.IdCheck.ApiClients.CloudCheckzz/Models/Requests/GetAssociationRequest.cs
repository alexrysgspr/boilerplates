using Si.IdCheck.ApiClients.CloudCheckzz.Helpers;

namespace Si.IdCheck.ApiClients.CloudCheckzz.Models.Requests;
public class GetAssociationRequest
{
    /// <summary>
    /// Unique CloudCheck identifier for an association.
    /// </summary>
    public string AssociationReference { get; set; }
}

public static class GetAssociationsRequestExtensions
{
    public static string ToQueryParams(this GetAssociationRequest request, string path, string apiKey, string apiSecret)
    {
        var nonce = CloudCheckHelpers.CreateNonce();

        var timestamp = CloudCheckHelpers.CreateUnixTimestamp();

        var parameters = new SortedDictionary<string, string>
        {
            ["key"] = apiKey,
            ["nonce"] = nonce,
            ["timestamp"] = timestamp,
            ["associationReference"] = request.AssociationReference
        };

        var signature = CloudCheckHelpers.CreateSignature(parameters, path, apiSecret);

        return
            $"?key={apiKey}&nonce={nonce}&timestamp={timestamp}&signature={signature}&associationReference={request.AssociationReference}";
    }
}