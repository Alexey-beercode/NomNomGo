using NomNomGo.IdentityService.Application.DTOs.Response.Users;
using NomNomGo.IdentityService.Application.Interfaces.CQRS;
using NomNomGo.IdentityService.Application.Models;

namespace NomNomGo.IdentityService.Application.UseCases.Users.Commands.BlockUser;

public record BlockUserCommand : ICommand<Result<UserDetailResponse>>
{
    public Guid UserId { get; init; }
    public string? Reason { get; init; }
    public string? Duration { get; init; } // "1h", "1d", "7d", "30d", "permanent"
}