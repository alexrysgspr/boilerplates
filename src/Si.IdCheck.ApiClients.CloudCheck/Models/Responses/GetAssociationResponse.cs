namespace Si.IdCheck.ApiClients.Verifidentity.Models.Responses;

public class GetAssociationResponse
{
    public string Reference { get; set; }
    public string AssociationReference { get; set; }
    public string VendorId { get; set; }
    public string CreatedBy { get; set; }
    public string Type { get; set; }
    /// <summary>
    /// The personDetail element will be present if type is PERSON. The entityDetail element will be present if type is ENTITY.
    /// </summary>
    public PersonDetail PersonDetail { get; set; }
    public EntityDetail EntityDetail { get; set; }
    public GetAssociationResponseCountry Country { get; set; }
    public string WhitelistPeids { get; set; }
    public string CreatedDate { get; set; }
    public string DeletedDate { get; set; }
    /// <summary>
    /// Each association can be identified by the unique associationReference. The hasAlerts flag indicates that the association is in alert mode, if set to true.
    /// </summary>
    public bool HasAlerts { get; set; }
    public List<Match> Matches { get; set; }
}

public class GetAssociationResponseCountry
{
    public string Code { get; set; }
    public string Name { get; set; }
}

public class EntityDetail
{
    public string Name { get; set; }
}

public class LatestReview
{
    public string Decision { get; set; }
    public string Notes { get; set; }
    public string ReviewedBy { get; set; }
    public string ReviewDate { get; set; }
}

public class Match
{
    public string MatchId { get; set; }
    public string Name { get; set; }
    public int Peid { get; set; }
    public double Score { get; set; }
    public List<string> Variations { get; set; }
    public string Type { get; set; }
    public DateTime MatchDate { get; set; }
    public bool MatchValid { get; set; }
    public string InvalidReason { get; set; }
    public string Gender { get; set; }
    public bool Deceased { get; set; }
    public List<RiskType> RiskTypes { get; set; }
    public List<string> BirthDates { get; set; }
    public PrimaryCountry PrimaryCountry { get; set; }
    public string Title { get; set; }
    public LatestReview LatestReview { get; set; }
}

public class Name
{
    public string Given { get; set; }
    public string Middle { get; set; }
    public string Family { get; set; }
}

public class PersonDetail
{
    public Name Name { get; set; }
    public string BirthYear { get; set; }
    public string Gender { get; set; }
}

public class PrimaryCountry
{
    public string Code { get; set; }
    public string Name { get; set; }
}

public class RiskType
{
    public string Code { get; set; }
}

