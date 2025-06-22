namespace NomNomGo.IdentityService.Application.DTOs.Response.Users;

public class UserListItem
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsBlocked { get; set; }
    public IEnumerable<string> Roles { get; set; } = new List<string>();
    public DateTime CreatedAt { get; set; }
}