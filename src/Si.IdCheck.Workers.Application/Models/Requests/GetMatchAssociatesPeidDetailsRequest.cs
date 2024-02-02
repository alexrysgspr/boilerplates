using Ardalis.Result;
using MediatR;
using Si.IdCheck.ApiClients.CloudCheck.Models.Responses;

namespace Si.IdCheck.Workers.Application.Models.Requests;
public class GetMatchAssociatesPeidDetailsRequest : IRequest<Result<List<PeidLookupResponse>>>
{
    public int Peid { get; set; }
    public IList<AssociateDetails> Associates { get; set; }
}
