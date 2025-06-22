using NomNomGo.IdentityService.Application.DTOs.Response.Authentication;
using NomNomGo.IdentityService.Application.Interfaces.CQRS;
using NomNomGo.IdentityService.Application.Models;

namespace NomNomGo.IdentityService.Application.UseCases.Authentication.Commands.Login;

public record LoginCommand : ICommand<Result<LoginResponse>>
{
    public string Login { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}