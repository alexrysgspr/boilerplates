﻿using Azure.Data.Tables;
using Azure;

namespace Si.IdCheck.AzureTableStorage.Models;

public class AzureTableStorageEntity : ITableEntity
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}
