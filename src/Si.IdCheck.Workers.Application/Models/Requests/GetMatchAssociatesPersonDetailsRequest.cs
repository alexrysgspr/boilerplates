using Ardalis.Result;
using MediatR;
using Si.IdCheck.ApiClients.CloudCheck.Models.Responses;
using Si.IdCheck.Workers.Application.Models.Responses;

namespace Si.IdCheck.Workers.Application.Models.Requests;
public class GetMatchAssociatesPersonDetailsRequest : IRequest<Result<GetMatchAssociatesPersonDetailsResponse>>
{
    public IList<AssociateDetails> Associates { get; set; }
}
