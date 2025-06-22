using NomNomGo.IdentityService.Domain.Common;
using NomNomGo.IdentityService.Domain.Entities.Relationships;

namespace NomNomGo.IdentityService.Domain.Entities;

public class Permission : BaseEntity {
    public string Name { get; set; } = string.Empty;
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
