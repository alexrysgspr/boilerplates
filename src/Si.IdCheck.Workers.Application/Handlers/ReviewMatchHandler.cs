using Ardalis.Result;
using MediatR;
using Si.IdCheck.Workers.Application.Models.Requests;
using Si.IdCheck.Workers.Application.Reviewers;

namespace Si.IdCheck.Workers.Application.Handlers;

public class ReviewMatchHandler : IRequestHandler<ReviewMatch, Result>
{
    private readonly Func<string, IReviewer> _reviewer;

    public ReviewMatchHandler(
        Func<string, IReviewer> reviewer)
    {
        _reviewer = reviewer;
    }

    public async Task<Result> Handle(ReviewMatch request, CancellationToken cancellationToken)
    {
        await _reviewer(request.ClientId)
            .ReviewAsync(request, cancellationToken);

        return Result.Success();
    }
}