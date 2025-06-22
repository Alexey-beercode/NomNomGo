namespace NomNomGo.IdentityService.Application.DTOs.Response.Authentication;

public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public IEnumerable<string> Roles { get; set; } = new List<string>();
    public DateTime ExpiresAt { get; set; }
}