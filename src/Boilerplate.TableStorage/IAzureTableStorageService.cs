using Azure.Data.Tables;

namespace Boilerplate.TableStorage;
public interface IAzureTableStorageService<T> where T : class, ITableEntity, new()
{
    Task SetupAsync();
    Task<T> GetAsync(string partitionId, string rowKey, CancellationToken cancellationToken);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken);
    Task<IEnumerable<T>> GetAllByPartitionKeyAsync(string partitionKey, CancellationToken cancellationToken);
    Task<IEnumerable<T>> GetAllByQueryAsync(string filter, IEnumerable<string> select, CancellationToken cancellationToken);
    Task InsertAsync(T entity, CancellationToken cancellationToken);
    Task BulkInsertAsync(IEnumerable<T> entities, CancellationToken cancellationToken);
    Task BulkInsertOrMergeAsync(IEnumerable<T> entities, CancellationToken cancellationToken);
    Task BulkInsertOrReplaceAsync(IEnumerable<T> entities, CancellationToken cancellationToken);
    Task BulkDeleteAsync(IEnumerable<T> entities, CancellationToken cancellationToken);
    Task BulkUpdateAsync(IEnumerable<T> entities, CancellationToken cancellationToken);
    Task BulkRunOperationsAsync(IEnumerable<TableTransactionAction> tableOperations, CancellationToken cancellationToken);
    Task InsertOrMergeAsync(T entity, CancellationToken cancellationToken);
    Task InsertOrReplaceAsync(T entity, CancellationToken cancellationToken);
    Task ReplaceAsync(T entity, CancellationToken cancellationToken);
}