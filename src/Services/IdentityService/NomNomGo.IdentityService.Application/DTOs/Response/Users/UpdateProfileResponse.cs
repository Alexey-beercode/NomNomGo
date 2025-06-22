namespace NomNomGo.IdentityService.Application.DTOs.Response.Users;

public class UpdateProfileResponse
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime UpdatedAt { get; set; }
}