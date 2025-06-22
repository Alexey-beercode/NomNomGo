namespace NomNomGo.IdentityService.Application.DTOs.Response.ServiceTokens;

public class ServiceTokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public string TokenType { get; set; } = "Bearer";
    public string Scope { get; set; } = string.Empty;
}