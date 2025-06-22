using NomNomGo.IdentityService.Domain.Entities;
using NomNomGo.IdentityService.Domain.Interfaces.Repositories.Base;

namespace NomNomGo.IdentityService.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Интерфейс репозитория пользователей
    /// </summary>
    public interface IUserRepository : IRepository<User>
    {
        /// <summary>
        /// Получает пользователя по email
        /// </summary>
        /// <param name="email">Email пользователя</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Пользователь или null, если не найден</returns>
        Task<User> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Получает пользователя по имени пользователя
        /// </summary>
        /// <param name="username">Имя пользователя</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Пользователь или null, если не найден</returns>
        Task<User> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Получает пользователя с включенными ролями
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Пользователь с ролями или null, если не найден</returns>
        Task<User> GetWithRolesAsync(Guid userId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Получает всех пользователей с пагинацией и фильтрацией
        /// </summary>
        /// <param name="search">Поиск по имени пользователя или email</param>
        /// <param name="role">Фильтр по роли</param>
        /// <param name="isBlocked">Фильтр по статусу блокировки</param>
        /// <param name="page">Номер страницы</param>
        /// <param name="pageSize">Размер страницы</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Список пользователей</returns>
        Task<(IEnumerable<User> Users, int TotalCount)> GetUsersWithFiltersAsync(
            string? search = null,
            string? role = null,
            bool? isBlocked = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Получает заблокированных пользователей
        /// </summary>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Список заблокированных пользователей</returns>
        Task<IEnumerable<User>> GetBlockedUsersAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Проверяет, заблокирован ли пользователь в данный момент
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>True, если пользователь заблокирован</returns>
        Task<bool> IsUserBlockedAsync(Guid userId, CancellationToken cancellationToken = default);

    }
}