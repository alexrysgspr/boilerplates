using Si.IdCheck.ApiClients.Verifidentity.Helpers;

namespace Si.IdCheck.ApiClients.Verifidentity.Models.Requests;
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
        var nonce = VerifidentityHelpers.CreateNonce();

        var timestamp = VerifidentityHelpers.CreateUnixTimestamp();

        var parameters = new SortedDictionary<string, string>
        {
            ["key"] = apiKey,
            ["nonce"] = nonce,
            ["timestamp"] = timestamp,
            ["associationReference"] = request.AssociationReference
        };

        var signature = VerifidentityHelpers.CreateSignature(parameters, path, apiSecret);

        return
            $"?key={apiKey}&nonce={nonce}&timestamp={timestamp}&signature={signature}&associationReference={request.AssociationReference}";
    }
}