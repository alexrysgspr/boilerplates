namespace Boilerplate.Common.Pagination;

/// <summary>
/// The response object for paginated API requests.
/// </summary>
/// <typeparam name="TResult">The return data type for the results IEnumerable.</typeparam>
public class PaginatedResponse<TResult>
{
    public PaginatedResponse()
    {
        Metadata = new PaginationMetaData();
    }

    /// <summary>
    /// MetaData for the response
    /// </summary>
    public PaginationMetaData Metadata { get; set; }
    
    /// <summary>
    /// Gets or sets the results set.
    /// </summary>
    public IEnumerable<TResult> Results { get; set; } = new List<TResult>();
}