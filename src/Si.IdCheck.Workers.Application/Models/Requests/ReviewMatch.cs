using Ardalis.Result;
using MediatR;
using Si.IdCheck.ApiClients.CloudCheck.Models.Responses;
using Si.IdCheck.Workers.Application.Models.Responses;

namespace Si.IdCheck.Workers.Application.Models.Requests;
public class ReviewMatch : IRequest<Result>
{
    public string ClientId { get; set; }
    public GetAssociationResponse PersonOfInterest { get; set; }
    public MatchDetails MatchDetails { get; set; }
    public Match Match { get; set; }
    public GetMatchAssociatesPersonDetailsResponse MatchAssociates { get; set; }
}
