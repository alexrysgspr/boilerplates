namespace Si.IdCheck.ApiClients.CloudCheckzz.Models.Requests;

public abstract class CloudCheckPostRequestBase
{
    public string Key { get; set; }
    public string Nonce { get; set; }
    public string Signature { get; set; }
    public string TimeStamp { get; set; }
}

public interface IParameterBuilder
{
    public SortedDictionary<string, string> BuildParameter(SortedDictionary<string, string> baseDictionary);
}

public interface IPostRequestBuilder
{
    public CloudCheckPostRequestBase BuildPostRequest(string key, string nonce, string signature, string timestamp);
}