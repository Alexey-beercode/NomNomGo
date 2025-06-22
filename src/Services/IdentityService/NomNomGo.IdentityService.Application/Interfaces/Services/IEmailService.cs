using NomNomGo.IdentityService.Domain.Models;

namespace NomNomGo.IdentityService.Application.Interfaces.Services;

public interface IEmailService
{
    Task SendRegistrationEmailAsync(RegistrationEmail registrationEmail, CancellationToken cancellationToken = default);
    Task SendOrderNotificationAsync(OrderEmail orderEmail, CancellationToken cancellationToken = default);
}