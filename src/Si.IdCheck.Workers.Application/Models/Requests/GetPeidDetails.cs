using Ardalis.Result;
using MediatR;
using Si.IdCheck.ApiClients.CloudCheck.Models.Responses;

namespace Si.IdCheck.Workers.Application.Models.Requests;
public class GetPeidDetails : IRequest<Result<PeidLookupResponse>>
{
    public int Peid { get; set; }
}
