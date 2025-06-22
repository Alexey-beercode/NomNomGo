using NomNomGo.IdentityService.Application.DTOs.Response.Users;
using NomNomGo.IdentityService.Application.Interfaces.CQRS;
using NomNomGo.IdentityService.Application.Models;

namespace NomNomGo.IdentityService.Application.UseCases.Users.Commands.UpdateProfile;

public record UpdateProfileCommand : ICommand<Result<UpdateProfileResponse>>
{
    // UserId будет извлечен из HTTP контекста (из токена)
    public string? Username { get; init; }
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
}