namespace Si.IdCheck.ApiClients.Verifidentity.Models.Responses;
public class VerifidentityResponse
{
    public Verification Verification { get; set; }
}

public class Verification
{
    public int? Error { get; set; }
    public string Message { get; set; }
}