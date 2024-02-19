using Azure;
using Azure.Data.Tables;

namespace Boilerplate.TableStorage.Models;

public static class AzureTableStorageEntityConsts
{
    public const string TableName = nameof(AzureTableStorageEntity);
}
public class AzureTableStorageEntity : ITableEntity
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}
