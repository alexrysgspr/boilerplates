using Si.IdCheck.ApiClients.CloudCheck.Models.Responses;

namespace Si.IdCheck.Workers.Application.Models.Responses;

public class GetMatchAssociatesPersonDetailsResponse
{
    public List<PeidLookupResponse> AssociatesInRelationshipFilter { get; set; }
    public List<AssociateDetails> AssociatesNotInInRelationshipFilter { get; set; }
}