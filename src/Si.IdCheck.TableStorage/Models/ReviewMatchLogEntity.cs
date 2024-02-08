namespace Si.IdCheck.AzureTableStorage.Models;
public static class ReviewMatchLogConsts
{
    public const string TableName = "ReviewMatchLogs";
}

public class ReviewMatchLogEntity : AzureTableStorageEntity
{
    public ReviewMatchLogEntity()
    {
        
    }

    public ReviewMatchLogEntity(string associationReference, string matchId, string reason, bool isCleared)
    {
        RowKey = matchId;
        PartitionKey = associationReference;
        AssociationReference = associationReference;
        MatchId = matchId;
        Reason = reason;
        IsCleared = isCleared;
    }

    public string AssociationReference { get; set; }
    public string MatchId { get; set; }
    public string Reason { get; set; }
    public bool IsCleared { get; set; }
}
