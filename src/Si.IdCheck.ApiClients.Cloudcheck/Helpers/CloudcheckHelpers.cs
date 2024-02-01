using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Si.IdCheck.ApiClients.Cloudcheck.Models.Requests;

namespace Si.IdCheck.ApiClients.Cloudcheck.Helpers;
public static class CloudcheckHelpers
{
    public static string CreateNonce()
    {
        var byteArray = new byte[20];
        using var random = RandomNumberGenerator.Create();

        random.GetBytes(byteArray);

        return Convert.ToBase64String(byteArray);
    }

    public static string CreateUnixTimestamp()
    {
        return (DateTimeOffset.Now.ToUnixTimeSeconds() * 1000).ToString();
    }

    public static string CreateSignature(SortedDictionary<string, string> parameters, string path, string secret)
    {
        var signatureString = new StringBuilder();

        signatureString.Append(path);

        foreach (var (s, value) in parameters)
        {
            signatureString.Append(s);
            signatureString.Append("=");
            signatureString.Append(value);
            signatureString.Append(";");
        }

        var encoding = new UTF8Encoding();
        var keyByte = encoding.GetBytes(secret);
        var messageBytes = encoding.GetBytes(signatureString.ToString());

        using var hmacsha256 = new HMACSHA256(keyByte);

        var hashedMessage = hmacsha256.ComputeHash(messageBytes);

        var signatureHex = new StringBuilder();

        for (var i = 0; i < hashedMessage.Length; i++)
        {
            signatureHex.Append(hashedMessage[i].ToString("X2"));
        }

        return signatureHex.ToString().ToLower();
    }

    public static CloudCheckPostRequestBase CreatePostRequest<T>(T request, string path, string apiKey, string apiSecret)
        where T : IParameterBuilder, IPostRequestBuilder
    {
        var nonce = CreateNonce();

        var timestamp = CreateUnixTimestamp();

        var parameters = CreateParameters(request, apiKey, nonce, timestamp);

        var signature = CreateSignature(parameters, path, apiSecret);

        var result = request.BuildPostRequest(apiKey, nonce, signature, timestamp);

        return result;
    }

    public static Dictionary<string, string> ToDictionary(this CloudCheckPostRequestBase request)
    {
        if (request == null)
        {
            return new Dictionary<string, string>();
        }

        var result = new Dictionary<string, string>();

        var requestType = request.GetType();

        var jsonString = JsonSerializer.Serialize(request, requestType);

        var initialDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString);

        foreach (var entry in initialDictionary)
        {
            result.Add(entry.Key, entry.Value?.ToString());
        }

        return result;
    }

    public static Dictionary<string, string> ToLowerCaseKeys(this IDictionary<string, string> dictionary)
    {
        if (dictionary == null)
        {
            return new Dictionary<string, string>();
        }

        return dictionary
            .Select(kvp => new KeyValuePair<string, string>(kvp.Key.ToLower(), kvp.Value))
            .ToDictionary();
    }

    public static SortedDictionary<string, string> CreateParameters<T>(T request, string key, string nonce, string timestamp) 
        where T : IParameterBuilder
    {
        var baseDictionary = new SortedDictionary<string, string>
        {
            ["key"] = key,
            ["nonce"] = nonce,
            ["timestamp"] = timestamp
        };

        var result = request.BuildParameter(baseDictionary);

        return result;
    }
}
