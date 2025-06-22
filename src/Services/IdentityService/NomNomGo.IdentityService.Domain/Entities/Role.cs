﻿using NomNomGo.IdentityService.Domain.Common;
using NomNomGo.IdentityService.Domain.Entities.Relationships;

namespace NomNomGo.IdentityService.Domain.Entities;

public class Role : BaseEntity {
    public string Name { get; set; } = string.Empty;
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
