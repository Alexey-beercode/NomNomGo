using NomNomGo.IdentityService.Application.Interfaces.CQRS;
using NomNomGo.IdentityService.Application.Models;

namespace NomNomGo.IdentityService.Application.UseCases.Users.Commands.ChangePassword
{
    public record ChangePasswordCommand : ICommand<Result>
    {
        // UserId будет извлечен из HTTP контекста (из токена)
        public string CurrentPassword { get; init; } = string.Empty;
        public string NewPassword { get; init; } = string.Empty;
    }
}