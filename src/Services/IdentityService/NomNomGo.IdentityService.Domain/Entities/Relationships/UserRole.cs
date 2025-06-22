using NomNomGo.IdentityService.Domain.Common;

namespace NomNomGo.IdentityService.Domain.Entities.Relationships;

public class UserRole : BaseEntity {
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;
}