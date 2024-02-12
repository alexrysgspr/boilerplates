
using Si.IdCheck.ApiClients.CloudCheck.Helpers;

namespace Si.IdCheck.ApiClients.CloudCheck.Models.Requests;
public class GetAssociationsRequest
{
    /// <summary>
    /// The number of associations to return per request. Must be > 0 and <= 10000. Defaults to 1000.
    /// </summary>
    public int PageSize { get; set; }
    /// <summary>
    /// Return associations in alert status only if set to true. Defaults to false, meaning that associations are returned regardless of their alert status.
    /// </summary>
    public bool FilterAlertOnly { get; set; }
    /// <summary>
    /// Return associations that have been deleted if set to true. Defaults to false, meaning that active associations are returned.
    /// </summary>
    public bool FilterIsDeleted { get; set; }
    /// <summary>
    /// Controls pagination. Can be omitted or set to "0" for the first call and set to nextCursor for subsequent calls (nextCursor returned in response meta if more data is available).
    /// </summary>
    public int Cursor { get; set; }
}

public static class GetAssociationRequestExtensions 
{
    public static string ToQueryParams(this GetAssociationsRequest request,  string path, string apiKey, string apiSecret)
    {
        var nonce = CloudCheckHelpers.CreateNonce();

        var timestamp = CloudCheckHelpers.CreateUnixTimestamp();

        var parameters = new SortedDictionary<string, string>
        {
            ["key"] = apiKey,
            ["nonce"] = nonce,
            ["timestamp"] = timestamp,
            ["pageSize"] = request.PageSize.ToString(),
            ["filterAlertOnly"] = request.FilterAlertOnly.ToString().ToLower(),
            ["filterIsDeleted"] = request.FilterIsDeleted.ToString().ToLower(),
            ["cursor"] = request.Cursor.ToString()
        };

        var signature = CloudCheckHelpers.CreateSignature(parameters, path, apiSecret);

        return
            $"?key={apiKey}&nonce={nonce}&timestamp={timestamp}&signature={signature}&pageSize={request.PageSize}&filterAlertOnly={request.FilterAlertOnly.ToString().ToLower()}&filterIsDeleted={request.FilterIsDeleted.ToString().ToLower()}&cursor={request.Cursor}";
    }

}
