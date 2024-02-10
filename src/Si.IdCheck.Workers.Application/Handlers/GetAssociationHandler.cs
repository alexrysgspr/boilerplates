using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Options;
using Si.IdCheck.ApiClients.CloudCheck;
using Si.IdCheck.ApiClients.CloudCheck.Models.Requests;
using Si.IdCheck.ApiClients.CloudCheck.Models.Responses;
using Si.IdCheck.Workers.Application.Models.Requests;
using Si.IdCheck.Workers.Application.Reviewers;

namespace Si.IdCheck.Workers.Application.Handlers;
public class GetAssociationHandler : IRequestHandler<GetAssociation, Result<GetAssociationResponse>>
{
    private readonly ICloudCheckApiClient _client;
    private readonly IOptionsSnapshot<ReviewerSettings> _settingsFactory;

    public GetAssociationHandler(
        ICloudCheckApiClient client,
        IOptionsSnapshot<ReviewerSettings> settingsFactory)
    {
        _settingsFactory = settingsFactory;
        _client = client;
    }

    public async Task<Result<GetAssociationResponse>> Handle(GetAssociation request, CancellationToken cancellationToken)
    {
        var settings = _settingsFactory.Get(request.ClientId);
        var cloudCheckRequest = new GetAssociationRequest
        {
            AssociationReference = request.AssociationReference
        };

        var association = await _client.GetAssociationAsync(cloudCheckRequest, settings.ApiKey,
            settings.ApiSecret);

        association.Matches = association
            .Matches
            .Where(match => settings.RelationshipTypes.Contains(match.Type, StringComparer.InvariantCultureIgnoreCase) && !settings.RiskTypes.All(y => match.RiskTypes.Select(rt => rt.Code).Contains(y, StringComparer.InvariantCultureIgnoreCase)))
            .ToList();

        return Result.Success(association);
    }
}
