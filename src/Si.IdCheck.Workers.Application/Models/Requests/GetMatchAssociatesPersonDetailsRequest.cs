using Ardalis.Result;
using MediatR;
using Si.IdCheck.ApiClients.CloudCheck.Models.Responses;

namespace Si.IdCheck.Workers.Application.Models.Requests;
public class GetMatchAssociatesPersonDetailsRequest : IRequest<Result<List<PeidLookupResponse>>>
{
    public IList<AssociateDetails> Associates { get; set; }
}
