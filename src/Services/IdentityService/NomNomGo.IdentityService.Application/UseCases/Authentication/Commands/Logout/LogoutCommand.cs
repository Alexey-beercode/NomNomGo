using NomNomGo.IdentityService.Application.Interfaces.CQRS;
using NomNomGo.IdentityService.Application.Models;

namespace NomNomGo.IdentityService.Application.UseCases.Authentication.Commands.Logout
{
    public record LogoutCommand : ICommand<Result>
    {
        public string RefreshToken { get; init; } = string.Empty;
    }
}