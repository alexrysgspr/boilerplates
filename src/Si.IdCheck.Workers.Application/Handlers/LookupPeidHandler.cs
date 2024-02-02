using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Options;
using Si.IdCheck.ApiClients.CloudCheck;
using Si.IdCheck.ApiClients.CloudCheck.Models.Requests;
using Si.IdCheck.ApiClients.CloudCheck.Models.Responses;
using Si.IdCheck.Workers.Application.Models.Requests;

namespace Si.IdCheck.Workers.Application.Handlers;
public class LookupPeidHandler : IRequestHandler<LookupPeid, Result<PeidLookupResponse>>
{
    private readonly ICloudCheckApiClient _client;
    private readonly CloudCheckSettings _cloudCheckSettings;

    public LookupPeidHandler(
        ICloudCheckApiClient client,
        IOptions<CloudCheckSettings> CloudCheckSettingsOption)
    {
        _client = client;
        _cloudCheckSettings = CloudCheckSettingsOption.Value;
    }

    public async Task<Result<PeidLookupResponse>> Handle(LookupPeid request, CancellationToken cancellationToken)
    {

        var CloudCheckRequest = new PeidLookupRequest
        {
            Peid = request.Peid
        };

        var peid = await _client.LookupPeidAsync(CloudCheckRequest, _cloudCheckSettings.ApiKey,
            _cloudCheckSettings.ApiSecret);

        return Result.Success(peid);
    }
}