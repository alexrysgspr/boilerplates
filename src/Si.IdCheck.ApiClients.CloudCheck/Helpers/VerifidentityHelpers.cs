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

    public static VerifidentityRequest CreatePostRequest<T>(T request, string path, string apiKey, string apiSecret)
    {
        var options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var data = JsonSerializer.Serialize(request, options);

        var nonce = CreateNonce();

        var timestamp = CreateUnixTimestamp();

        var parameters = new SortedDictionary<string, string>
        {
            ["key"] = apiKey,
            ["nonce"] = nonce,
            ["timestamp"] = timestamp,
            ["data"] = data
        };

        var signature = CreateSignature(parameters, path, apiSecret);

        return new VerifidentityRequest
        {
            Key = apiKey,
            Data = data,
            Nonce = nonce,
            Signature = signature,
            TimeStamp = timestamp
        };
    }

    public static Dictionary<string, string> ToDictionary(this VerifidentityRequest request)
    {
        var json = JsonSerializer.Serialize(request);
        var dictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

        return dictionary.ToCamelCaseKeys();
    }

    public static Dictionary<string, string> ToCamelCaseKeys(this IDictionary<string, string> dictionary)
    {
        return dictionary
            .Select(kvp => new KeyValuePair<string, string>(char.ToLower(kvp.Key[0]) + kvp.Key[1..], kvp.Value))
            .ToDictionary();
    }
}
