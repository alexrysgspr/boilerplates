using Si.IdCheck.Workers.Application.Models.Requests;

namespace Si.IdCheck.Workers.Application.Reviewers;

public interface IReviewer
{
    Task ReviewAsync(ReviewMatch request, CancellationToken cancellationToken);
}