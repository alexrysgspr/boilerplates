using Ardalis.Result;
using MediatR;
using Si.IdCheck.ApiClients.CloudCheck.Models.Responses;

namespace Si.IdCheck.Workers.Application.Models.Requests;
public class ReviewMatch : IRequest<Result>
{
    public string AssociationReference { get; set; }
    public PeidLookupResponse Associate { get; set; }
    public MatchDetails MatchDetails { get; set; }
    public Match Match { get; set; }
    public List<PeidLookupResponse> Associates { get; set; }
}
