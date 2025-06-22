using NomNomGo.IdentityService.Domain.Entities;
using NomNomGo.IdentityService.Domain.Interfaces.Repositories.Base;

namespace NomNomGo.IdentityService.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Интерфейс репозитория ролей
    /// </summary>
    public interface IRoleRepository : IRepository<Role>
    {
        /// <summary>
        /// Получает роль по имени
        /// </summary>
        /// <param name="name">Название роли</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Роль или null, если не найдена</returns>
        Task<Role> GetByNameAsync(string name, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Получает роль со всеми связанными разрешениями
        /// </summary>
        /// <param name="roleId">Идентификатор роли</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Роль с разрешениями или null, если не найдена</returns>
        Task<Role> GetWithPermissionsAsync(Guid roleId, CancellationToken cancellationToken = default);
    }
}