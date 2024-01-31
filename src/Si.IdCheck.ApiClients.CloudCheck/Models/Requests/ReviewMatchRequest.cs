namespace Si.IdCheck.ApiClients.Verifidentity.Models.Requests;
public class ReviewMatchRequest
{
    public string AssociationReference { get; set; }
    public Review Review { get; set; }
}

public class Review
{
    /// <summary>
    /// The matchId is the unique identifier for a match within Cloudcheck.
    /// </summary>
    public string MatchId { get; set; }
    public string Decision { get; set; }
    public string Notes { get; set; }
}
