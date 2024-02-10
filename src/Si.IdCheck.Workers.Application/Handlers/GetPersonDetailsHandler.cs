using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Options;
using Si.IdCheck.ApiClients.CloudCheck;
using Si.IdCheck.ApiClients.CloudCheck.Models.Requests;
using Si.IdCheck.ApiClients.CloudCheck.Models.Responses;
using Si.IdCheck.Workers.Application.Models.Requests;
using Si.IdCheck.Workers.Application.Reviewers;

namespace Si.IdCheck.Workers.Application.Handlers;
public class GetPersonDetailsHandler : IRequestHandler<GetPersonDetails, Result<PeidLookupResponse>>
{
    private readonly ICloudCheckApiClient _client;
    private readonly IOptionsSnapshot<ReviewerSettings> _settingsFactory;

    public GetPersonDetailsHandler(
        ICloudCheckApiClient client,
        IOptionsSnapshot<ReviewerSettings> settingsFactory)
    {
        _client = client;
        _settingsFactory = settingsFactory;
    }

    public async Task<Result<PeidLookupResponse>> Handle(GetPersonDetails request, CancellationToken cancellationToken)
    {
        var settings = _settingsFactory.Get(request.ClientId);

        var cloudCheckRequest = new PeidLookupRequest
        {
            Peid = request.Peid
        };

        var result = await _client.LookupPeidAsync(cloudCheckRequest, settings.ApiKey,
            settings.ApiSecret);

        result.Response.Matches ??= new List<MatchDetails>();

        return Result.Success(result);
    }
}