using NomNomGo.IdentityService.Domain.Entities;
using NomNomGo.IdentityService.Domain.Interfaces.Repositories.Base;

namespace NomNomGo.IdentityService.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Интерфейс репозитория разрешений
    /// </summary>
    public interface IPermissionRepository : IRepository<Permission>
    {
        /// <summary>
        /// Получает разрешение по имени
        /// </summary>
        /// <param name="name">Название разрешения</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Разрешение или null, если не найдено</returns>
        Task<Permission> GetByNameAsync(string name, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Получает все разрешения для указанной роли
        /// </summary>
        /// <param name="roleId">Идентификатор роли</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Список разрешений</returns>
        Task<IReadOnlyList<Permission>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);
    }
}