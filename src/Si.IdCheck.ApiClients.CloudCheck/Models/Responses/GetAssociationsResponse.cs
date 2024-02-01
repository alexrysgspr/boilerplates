namespace Si.IdCheck.ApiClients.Verifidentity.Models.Responses;
public class GetAssociationsResponse
{
    public List<Association> Associations { get; set; }
    public Meta Meta { get; set; }
}

public class Association
{
    public string Reference { get; set; }
    /// <summary>
    /// Each association can be identified by the unique associationReference. The hasAlerts flag indicates that the association is in alert mode, if set to true.
    /// </summary>
    public string AssociationReference { get; set; }
    public string VendorId { get; set; }
    public string CreatedBy { get; set; }
    public string Type { get; set; }
    /// <summary>
    /// The personDetail element will be present if type is PERSON. The entityDetail element will be present if type is ENTITY.
    /// </summary>
    public PersonDetail PersonDetail { get; set; }
    public EntityDetail EntityDetail { get; set; }
    public string CountryCode { get; set; }
    public string WhitelistPeids { get; set; }
    public string CreatedDate { get; set; }
    public string DeletedDate { get; set; }
    /// <summary>
    /// Each association can be identified by the unique associationReference. The hasAlerts flag indicates that the association is in alert mode, if set to true.
    /// </summary>
    public bool HasAlerts { get; set; }
}



public class Meta
{
    /// <summary>
    /// If the nextCursor element is returned in the meta element it means that more associations are available for download. To fetch them, repeat the Get Associations API call, setting the cursor value to the value of nextCursor.
    /// </summary>
    public string NextCursor { get; set; }
    public List<string> Messages { get; set; }
}
