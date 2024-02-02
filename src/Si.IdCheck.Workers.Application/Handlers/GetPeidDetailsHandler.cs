using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Options;
using Si.IdCheck.ApiClients.CloudCheck;
using Si.IdCheck.ApiClients.CloudCheck.Models.Requests;
using Si.IdCheck.ApiClients.CloudCheck.Models.Responses;
using Si.IdCheck.Workers.Application.Models.Requests;

namespace Si.IdCheck.Workers.Application.Handlers;
public class GetPeidDetailsHandler : IRequestHandler<GetPeidDetails, Result<PeidLookupResponse>>
{
    private readonly ICloudCheckApiClient _client;
    private readonly CloudCheckSettings _cloudCheckSettings;

    public GetPeidDetailsHandler(
        ICloudCheckApiClient client,
        IOptions<CloudCheckSettings> cloudCheckSettingsOption)
    {
        _client = client;
        _cloudCheckSettings = cloudCheckSettingsOption.Value;
    }

    public async Task<Result<PeidLookupResponse>> Handle(GetPeidDetails request, CancellationToken cancellationToken)
    {

        var cloudCheckRequest = new PeidLookupRequest
        {
            Peid = request.Peid
        };

        var peid = await _client.LookupPeidAsync(cloudCheckRequest, _cloudCheckSettings.ApiKey,
            _cloudCheckSettings.ApiSecret);

        return Result.Success(peid);
    }
}