using Ardalis.Result;
using MediatR;
using Si.IdCheck.ApiClients.CloudCheck.Models.Responses;

namespace Si.IdCheck.Workers.Application.Models.Requests;
public class GetAssociations : IRequest<Result<List<Association>>>
{
    //todo: in case we change the implementation to tenant-based credentials
    //public string TenantId { get; set; }
}
