using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Options;
using Si.IdCheck.ApiClients.Cloudcheck;
using Si.IdCheck.ApiClients.Cloudcheck.Models.Requests;
using Si.IdCheck.ApiClients.Cloudcheck.Models.Responses;
using Si.IdCheck.Workers.Application.Models.Requests;

namespace Si.IdCheck.Workers.Application.Handlers;
public class LookupPeidHandler : IRequestHandler<LookupPeid, Result<PeidLookupResponse>>
{
    private readonly ICloudcheckApiClient _client;
    private readonly CloudcheckSettings _CloudcheckSettings;

    public LookupPeidHandler(
        ICloudcheckApiClient client,
        IOptions<CloudcheckSettings> CloudcheckSettingsOption)
    {
        _client = client;
        _CloudcheckSettings = CloudcheckSettingsOption.Value;
    }

    public async Task<Result<PeidLookupResponse>> Handle(LookupPeid request, CancellationToken cancellationToken)
    {

        var CloudcheckRequest = new PeidLookupRequest
        {
            Peid = request.Peid
        };

        var peid = await _client.LookupPeidAsync(CloudcheckRequest, _CloudcheckSettings.ApiKey,
            _CloudcheckSettings.ApiSecret);

        return Result.Success(peid);
    }
}