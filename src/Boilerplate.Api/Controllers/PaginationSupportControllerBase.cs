using Boilerplate.Common.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace Boilerplate.Api.Controllers;

/// <summary>
/// Controller Base which provides pagination support
/// </summary>
public abstract class PaginationSupportControllerBase : ControllerBase
{
    /// <summary>
    ///     Creates the navigation links for a paginated API response.
    /// </summary>
    /// <param name="actionName">The name of the current action.</param>
    /// <param name="request">The paginated API request.</param>
    /// <param name="response">The paginated API response.</param>
    /// <typeparam name="TResult">The type of the results IEnumerable in the paginated API response.</typeparam>
    /// <returns>The pagination links.</returns>
    protected Links CreatePaginationLinks<TResult>(
        string actionName,
        PaginatedRequest<TResult> request,
        PaginatedResponse<TResult> response)
    {
        return new()
        {
            Self = CreateLinkRelativeUri(actionName, request),
            Next = CreateNextLink(actionName, request, response),
            Previous = CreatePreviousLink(actionName, request, response)
        };
    }

    /// <summary>
    ///     Creates the navigation link for the next page of results.
    /// </summary>
    /// <param name="actionName">The name of the current action.</param>
    /// <param name="request">The paginated API request.</param>
    /// <param name="response">The paginated API response.</param>
    /// <typeparam name="TResult">The type of the results IEnumerable in the paginated API response.</typeparam>
    /// <returns>The link to the next page if the next page exists.</returns>
    private Uri? CreateNextLink<TResult>(
        string actionName,
        PaginatedRequest<TResult> request,
        PaginatedResponse<TResult> response)
    {
        if (request.Offset + request.Limit >= response.Metadata.Total) return null;

        request.Offset += request.Limit;

        var next = CreateLinkRelativeUri(actionName, request);

        request.Offset -= request.Limit;

        return next;
    }

    /// <summary>
    ///     Creates the navigation link for the previous page of results.
    /// </summary>
    /// <param name="actionName">The name of the current action.</param>
    /// <param name="request">The paginated API request.</param>
    /// <param name="response">The paginated API response.</param>
    /// <typeparam name="TResult">The type of the results IEnumerable in the paginated API response.</typeparam>
    /// <returns>The link to the previous page if the previous page exists.</returns>
    private Uri? CreatePreviousLink<TResult>(
        string actionName,
        PaginatedRequest<TResult> request,
        PaginatedResponse<TResult> response)
    {
        if (request.Offset <= 0) return null;

        if (request.Offset <= request.Limit)
        {
            request.Offset = 0;
            return CreateLinkRelativeUri(actionName, request);
        }

        if (request.Offset > response.Metadata.Total)
        {
            request.Offset = response.Metadata.Total - request.Limit;
            return CreateLinkRelativeUri(actionName, request);
        }

        request.Offset -= request.Limit;
        return CreateLinkRelativeUri(actionName, request);
    }

    /// <summary>
    ///     Creates the relative URI for the paginated request.
    /// </summary>
    /// <param name="actionName">The name of the current action.</param>
    /// <param name="request">The paginated API request.</param>
    /// <typeparam name="TResult">The type of the results IEnumerable in the paginated API response.</typeparam>
    /// <returns>The relative URI.</returns>
    private Uri CreateLinkRelativeUri<TResult>(string actionName, PaginatedRequest<TResult> request)
    {
        var action = Url.Action(actionName, request);
        if (action != null) return new Uri(action, UriKind.Relative);

        throw new ArgumentException($"{actionName} is an action name which does not exist");
    }
}