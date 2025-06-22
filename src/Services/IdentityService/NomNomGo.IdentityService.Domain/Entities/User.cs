using NomNomGo.IdentityService.Domain.Common;
using NomNomGo.IdentityService.Domain.Entities.Relationships;

namespace NomNomGo.IdentityService.Domain.Entities;

public class User : BaseEntity {
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsBlocked { get; set; } = false;
    public DateTime? BlockedUntil { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}