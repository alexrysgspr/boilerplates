namespace Si.IdCheck.ApiClients.Cloudcheck.Models.Requests;

public abstract class CloudcheckPostRequestBase
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
    public CloudcheckPostRequestBase BuildPostRequest(string key, string nonce, string signature, string timestamp);
}