using NomNomGo.IdentityService.Application.DTOs.Response.Users;
using NomNomGo.IdentityService.Application.Interfaces.CQRS;
using NomNomGo.IdentityService.Application.Models;

namespace NomNomGo.IdentityService.Application.UseCases.Users.Commands.UnblockUser;

public record UnblockUserCommand : ICommand<Result<UserDetailResponse>>
{
    public Guid UserId { get; init; }
}