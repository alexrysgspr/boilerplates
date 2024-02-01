using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Options;
using Si.IdCheck.ApiClients.Verifidentity;
using Si.IdCheck.ApiClients.Verifidentity.Models.Requests;
using Si.IdCheck.ApiClients.Verifidentity.Models.Responses;
using Si.IdCheck.Workers.Application.Models.Requests;

namespace Si.IdCheck.Workers.Application.Handlers;
public class LookupPeidHandler : IRequestHandler<LookupPeid, Result<PeidLookupResponse>>
{
    private readonly IVerifidentityApiClient _client;
    private readonly VerifidentitySettings _verifidentitySettings;

    public LookupPeidHandler(
        IVerifidentityApiClient client,
        IOptions<VerifidentitySettings> verifidentitySettingsOption)
    {
        _client = client;
        _verifidentitySettings = verifidentitySettingsOption.Value;
    }

    public async Task<Result<PeidLookupResponse>> Handle(LookupPeid request, CancellationToken cancellationToken)
    {

        var verifidentityRequest = new PeidLookupRequest
        {
            Peid = request.Peid
        };

        var peid = await _client.LookupPeidAsync(verifidentityRequest, _verifidentitySettings.ApiKey,
            _verifidentitySettings.ApiSecret);

        return Result.Success(peid);
    }
}