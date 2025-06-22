using NomNomGo.IdentityService.Domain.Common;

namespace NomNomGo.IdentityService.Domain.Entities;

public class RefreshToken : BaseEntity {
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}