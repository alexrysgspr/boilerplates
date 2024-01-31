namespace Si.IdCheck.ApiClients.Verifidentity.Models.Requests;

public class PeidLookupRequest : CloudCheckPostRequestBase
{
    public int Peid { get; set; }
}

