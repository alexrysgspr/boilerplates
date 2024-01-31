namespace Si.IdCheck.ApiClients.Verifidentity.Models.Requests;

public abstract class CloudCheckPostRequestBase
{
    public string Key { get; set; }
    public string Nonce { get; set; }
    public string Signature { get; set; }
    public string TimeStamp { get; set; }
}