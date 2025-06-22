using NomNomGo.IdentityService.Domain.Entities.Relationships;
using NomNomGo.IdentityService.Domain.Interfaces.Repositories.Base;

namespace NomNomGo.IdentityService.Domain.Interfaces.Repositories;

public interface IUserRoleRepository : IRepository<UserRole>
{
    Task<IReadOnlyList<UserRole>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserRole>> GetRoleUsersAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task<UserRole?> GetUserRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);
    Task<bool> UserHasRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken = default);
}