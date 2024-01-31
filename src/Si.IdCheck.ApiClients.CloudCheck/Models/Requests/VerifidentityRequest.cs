namespace Si.IdCheck.ApiClients.Verifidentity.Models.Requests;
public class VerifidentityRequest
{
    public string Data { get; set; }
    public string Key { get; set; }
    public string Nonce { get; set; }
    public string TimeStamp { get; set; }
    public string Signature { get; set; }
}

