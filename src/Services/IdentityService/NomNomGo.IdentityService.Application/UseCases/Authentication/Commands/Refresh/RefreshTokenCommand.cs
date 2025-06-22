using NomNomGo.IdentityService.Application.DTOs.Response.Authentication;
using NomNomGo.IdentityService.Application.Interfaces.CQRS;
using NomNomGo.IdentityService.Application.Models;

namespace NomNomGo.IdentityService.Application.UseCases.Authentication.Commands.Refresh;

public record RefreshTokenCommand : ICommand<Result<RefreshTokenResponse>>
{
    public string RefreshToken { get; init; } = string.Empty;
}