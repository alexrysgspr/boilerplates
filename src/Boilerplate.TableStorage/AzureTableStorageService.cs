using Azure.Data.Tables;

namespace Boilerplate.TableStorage;
public class AzureTableStorageService<T> : IAzureTableStorageService<T> where T : class, ITableEntity, new()
{
    private readonly TableClient _client;
    private readonly int _maxPerPage = 1000;

    
    public AzureTableStorageService(TableServiceClient client, string tableName)
    {
        _client = client.GetTableClient(tableName);
    }

    public async Task SetupAsync()
    {
        await _client.CreateIfNotExistsAsync();
    }

    
    public async Task<T> GetAsync(string partitionKey, string rowKey, CancellationToken cancellationToken)
    {
        var result = await _client.GetEntityAsync<T>(partitionKey, rowKey, null, cancellationToken);
        return result.Value;
    }

    public async Task<IEnumerable<T>> GetAllByQueryAsync(
        string filter,
        IEnumerable<string> select,
        CancellationToken cancellationToken)
    {
        var result = new List<T>();

        var tableQueryResult = _client.QueryAsync<T>(
            filter,
            _maxPerPage,
            select,
            cancellationToken);

        await foreach (var page in tableQueryResult.AsPages().WithCancellation(cancellationToken))
        {
            result.AddRange(page.Values);
        }

        return result;
    }

    public async Task<IEnumerable<T>> GetAllByPartitionKeyAsync(string partitionKey, CancellationToken cancellationToken)
    {
        return await GetAllByQueryAsync(
            $"PartitionKey eq '{partitionKey}'",
            default,
            cancellationToken);
    }

    public async Task InsertAsync(T entity, CancellationToken cancellationToken)
    {
        await _client.AddEntityAsync(entity, cancellationToken);
    }

    public async Task BulkInsertAsync(IEnumerable<T> entities, CancellationToken cancellationToken)
    {
        var batch = new List<TableTransactionAction>();

        foreach (var entity in entities)
        {
            batch.Add(new TableTransactionAction(TableTransactionActionType.Add, entity));
        }

        await BulkRunOperationsAsync(batch, cancellationToken);
    }

    public async Task BulkInsertOrMergeAsync(IEnumerable<T> entities, CancellationToken cancellationToken)
    {
        var batch = new List<TableTransactionAction>();

        foreach (var entity in entities)
        {
            batch.Add(new TableTransactionAction(TableTransactionActionType.UpsertMerge, entity));
        }

        await BulkRunOperationsAsync(batch, cancellationToken);
    }

    public async Task BulkUpdateAsync(IEnumerable<T> entities, CancellationToken cancellationToken)
    {
        var batch = new List<TableTransactionAction>();

        foreach (var entity in entities)
        {
            batch.Add(new TableTransactionAction(TableTransactionActionType.UpdateMerge, entity));
        }

        await BulkRunOperationsAsync(batch, cancellationToken);
    }

    public async Task BulkInsertOrReplaceAsync(IEnumerable<T> entities, CancellationToken cancellationToken)
    {
        var batch = new List<TableTransactionAction>();

        foreach (var entity in entities)
        {
            batch.Add(new TableTransactionAction(TableTransactionActionType.UpsertMerge, entity));
        }

        await BulkRunOperationsAsync(batch, cancellationToken);
    }

    public async Task BulkRunOperationsAsync(IEnumerable<TableTransactionAction> tableOperations, CancellationToken cancellationToken)
    {
        await _client.SubmitTransactionAsync(tableOperations, cancellationToken);
    }

    public async Task InsertOrMergeAsync(T entity, CancellationToken cancellationToken)
    {
        await _client.UpsertEntityAsync(entity, TableUpdateMode.Merge, cancellationToken);
    }

    public async Task BulkDeleteAsync(IEnumerable<T> entities, CancellationToken cancellationToken)
    {
        var batch = new List<TableTransactionAction>();

        foreach (var entity in entities)
        {
            batch.Add(new TableTransactionAction(TableTransactionActionType.Delete, entity));
        }

        await BulkRunOperationsAsync(batch, cancellationToken);
    }

    public async Task InsertOrReplaceAsync(T entity, CancellationToken cancellationToken)
    {
        await _client.UpsertEntityAsync(entity, TableUpdateMode.Replace, cancellationToken);
    }

    public async Task ReplaceAsync(T entity, CancellationToken cancellationToken)
    {
        await _client.UpdateEntityAsync(entity, entity.ETag, TableUpdateMode.Replace, cancellationToken);
    }

    private async Task ExecuteAsync(TableTransactionAction tableOperation, CancellationToken cancellationToken)
    {
        await BulkRunOperationsAsync(new List<TableTransactionAction> { tableOperation }, cancellationToken);
    }

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await GetAllByQueryAsync(
            string.Empty,
            default,
            cancellationToken);
    }
}