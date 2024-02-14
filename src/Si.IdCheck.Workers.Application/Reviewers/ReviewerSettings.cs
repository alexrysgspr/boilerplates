namespace Si.IdCheck.Workers.Application.Reviewers;
public class ReviewerSettings
{
    public bool ClearEnabled { get; set; }
    public string ApiKey { get; set; }
    public string ApiSecret { get; set; }
    public string[] RelationshipsToFilter { get; set; }
    public string[] RelationshipTypes { get; set; }
    public string[] RiskTypes { get; set; }
    public string[] AssociationTypes { get; set; }
}
