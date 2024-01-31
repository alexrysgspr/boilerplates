using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using Si.IdCheck.ApiClients.Verifidentity.Models.Requests;

namespace Si.IdCheck.ApiClients.Verifidentity.Helpers;
public static class VerifidentityHelpers
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
    {
        var options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var data = JsonSerializer.Serialize(request, options);

        var nonce = CreateNonce();

        var timestamp = CreateUnixTimestamp();

        var parameters = CreateParameters(request, apiKey, nonce, timestamp, data);

        var signature = CreateSignature(parameters, path, apiSecret);

        return CreatePostRequest(request, apiKey, nonce, signature, timestamp, data);
    }

    public static Dictionary<string, string> ToDictionary(this CloudCheckPostRequestBase request)
    {
        if (request == null)
        {
            return new Dictionary<string, string>();
        }

        var stringBuilder = new StringBuilder();
        var dictionary = new Dictionary<string, string>();

        switch (request)
        {
            case PeidLookupRequest peidRequest:
                stringBuilder.Append(JsonSerializer.Serialize(request));
                dictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(stringBuilder.ToString());
                dictionary["peid"] = peidRequest.Peid.ToString();
                break;
            case VerifidentityRequest verifyIdentityRequest:
                stringBuilder.Append(JsonSerializer.Serialize(verifyIdentityRequest));
                dictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(stringBuilder.ToString());
                break;
            default:
                break;
        }

        return dictionary;
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

    public static SortedDictionary<string, string> CreateParameters<T>(T request, string key, string nonce, string timestamp, string data)
    {
        var result = new SortedDictionary<string, string>
        {
            ["key"] = key,
            ["nonce"] = nonce,
            ["timestamp"] = timestamp
        };

        switch (request)
        {
            case PeidLookupRequest peidRequest:
                result.Add("peid", peidRequest.Peid.ToString());
                break;
            case ReviewMatchRequest:
                result.Add("data", data);
                break;
        }

        return result;
    }

    private static CloudCheckPostRequestBase CreatePostRequest<T>(T request, string key, string nonce, string signature, string timestamp, string data)
    {
        switch (request)
        {
            case PeidLookupRequest peidRequest:
                return new PeidLookupRequest
                {
                    Peid = peidRequest.Peid,
                    Key = key,
                    Nonce = nonce,
                    Signature = signature,
                    TimeStamp = timestamp
                };
            case ReviewMatchRequest verifyRequest:
                return new VerifidentityRequest
                {
                    Data = data,
                    Key = key,
                    Nonce = nonce,
                    Signature = signature,
                    TimeStamp = timestamp
                };
        }

        return null;
    }
}
