using Microsoft.EntityFrameworkCore;
using NomNomGo.IdentityService.Domain.Entities;
using NomNomGo.IdentityService.Domain.Interfaces.Repositories;
using NomNomGo.IdentityService.Infrastructure.Persistence.Database;
using NomNomGo.IdentityService.Infrastructure.Repositories.Base;

namespace NomNomGo.IdentityService.Infrastructure.Repositories
{
    /// <summary>
    /// Реализация репозитория ролей
    /// </summary>
    public class RoleRepository : BaseRepository<Role>, IRoleRepository
    {
        public RoleRepository(ApplicationDbContext dbContext) : base(dbContext) { }

        /// <inheritdoc />
        public async Task<Role> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Name.ToLower() == name.ToLower(), cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Role> GetWithPermissionsAsync(Guid roleId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);
        }
    }
}