namespace NomNomGo.IdentityService.Application.DTOs.Request;

public class RegistrationEmailRequest
{
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
}