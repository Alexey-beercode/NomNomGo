using Microsoft.EntityFrameworkCore;
using NomNomGo.IdentityService.Domain.Entities;
using NomNomGo.IdentityService.Domain.Entities.Relationships;
using NomNomGo.IdentityService.Domain.Interfaces;
using NomNomGo.IdentityService.Domain.Interfaces.Repositories;
using NomNomGo.IdentityService.Infrastructure.Persistence.Database;
using NomNomGo.IdentityService.Infrastructure.Repositories.Base;

namespace NomNomGo.IdentityService.Infrastructure.Repositories
{
    /// <summary>
    /// Реализация репозитория разрешений
    /// </summary>
    public class PermissionRepository : BaseRepository<Permission>, IPermissionRepository
    {
        public PermissionRepository(ApplicationDbContext dbContext) : base(dbContext) { }

        /// <inheritdoc />
        public async Task<Permission> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Name.ToLower() == name.ToLower(), cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<Permission>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<Permission>()
                .Join(
                    _dbContext.Set<RolePermission>(),
                    permission => permission.Id,
                    rolePermission => rolePermission.PermissionId,
                    (permission, rolePermission) => new { Permission = permission, RolePermission = rolePermission }
                )
                .Where(x => x.RolePermission.RoleId == roleId)
                .Select(x => x.Permission)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
    }
}