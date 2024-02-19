namespace Boilerplate.Common.Pagination;

public class PaginationMetaData
{
    /// <summary>
    /// Gets or sets the navigation links for pagination.
    /// </summary>
    public Links Links { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets the limit used in the original request.
    /// </summary>
    public int Limit { get; set; }
    /// <summary>
    /// Gets or sets the offset used in the original request.
    /// </summary>
    public int Offset { get; set; }
    /// <summary>
    /// Gets or sets the total number of records in the results set.
    /// </summary>
    public int Total { get; set; }
}