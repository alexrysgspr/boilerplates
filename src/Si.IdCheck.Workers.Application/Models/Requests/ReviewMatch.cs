using Ardalis.Result;
using MediatR;
using Si.IdCheck.ApiClients.Verifidentity.Models.Responses;

namespace Si.IdCheck.Workers.Application.Models.Requests;
public class ReviewMatch : IRequest<Result>
{
    public PeidLookupResponse Peid { get; set; }
    public GetAssociationResponse Association { get; set; }
    public Match Match { get; set; }
}
