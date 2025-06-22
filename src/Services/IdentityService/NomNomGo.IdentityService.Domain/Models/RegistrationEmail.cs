namespace NomNomGo.IdentityService.Domain.Models;

public class RegistrationEmail
{
    public string ToEmail { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string AppName { get; set; } = string.Empty;
}