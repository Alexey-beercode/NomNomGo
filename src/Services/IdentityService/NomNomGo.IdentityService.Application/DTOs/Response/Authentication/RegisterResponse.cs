namespace NomNomGo.IdentityService.Application.DTOs.Response.Authentication;

public class RegisterResponse
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}