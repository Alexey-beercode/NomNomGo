using NomNomGo.IdentityService.Application.DTOs.Response.Users;
using NomNomGo.IdentityService.Application.Interfaces.CQRS;
using NomNomGo.IdentityService.Application.Models;

namespace NomNomGo.IdentityService.Application.UseCases.Users.Queries.GetById;

public record GetUserByIdQuery : IQuery<Result<UserDetailResponse>>
{
    public Guid UserId { get; init; }
}