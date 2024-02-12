namespace Si.IdCheck.ApiClients.CloudCheckzz.Models.Responses;
public class CloudCheckResponse
{
    public Verification Verification { get; set; }
}

public class Verification
{
    public int? Error { get; set; }
    public string Message { get; set; }
}