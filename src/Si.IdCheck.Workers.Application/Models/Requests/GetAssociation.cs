using Ardalis.Result;
using MediatR;
using Si.IdCheck.ApiClients.Cloudcheck.Models.Responses;

namespace Si.IdCheck.Workers.Application.Models.Requests;
public class GetAssociation : IRequest<Result<GetAssociationResponse>>
{
    public string AssociationReference { get; set; }
}
