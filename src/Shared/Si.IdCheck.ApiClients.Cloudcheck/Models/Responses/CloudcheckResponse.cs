namespace Si.IdCheck.ApiClients.Cloudcheck.Models.Responses;
public class CloudcheckResponse
{
    public Verification Verification { get; set; }
}

public class Verification
{
    public int? Error { get; set; }
    public string Message { get; set; }
}