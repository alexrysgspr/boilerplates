﻿using Ardalis.Result;
using MediatR;
using Si.IdCheck.ApiClients.Verifidentity.Models.Responses;

namespace Si.IdCheck.Workers.Application.Models.Requests;
public class GetAssociations : IRequest<Result<List<Association>>>
{
}