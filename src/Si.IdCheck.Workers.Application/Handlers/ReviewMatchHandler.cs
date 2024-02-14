using Ardalis.Result;
using Ardalis.Result.FluentValidation;
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
        var validator = new ReviewMatchValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            return Result.Invalid(validationResult.AsErrors());
        }

        await _reviewer(request.ClientId)
            .ReviewAsync(request, cancellationToken);

        return Result.Success();
    }
}