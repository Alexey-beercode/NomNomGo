using NomNomGo.IdentityService.Domain.Entities;
using NomNomGo.IdentityService.Domain.Interfaces.Repositories.Base;

namespace NomNomGo.IdentityService.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Интерфейс репозитория токенов обновления
    /// </summary>
    public interface IRefreshTokenRepository : IRepository<RefreshToken>
    {
        /// <summary>
        /// Получает токен обновления по значению токена
        /// </summary>
        /// <param name="token">Значение токена</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Токен обновления или null, если не найден</returns>
        Task<RefreshToken> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Получает активные токены обновления для пользователя
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Список активных токенов обновления</returns>
        Task<IReadOnlyList<RefreshToken>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Удаляет все просроченные токены обновления
        /// </summary>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Количество удаленных токенов</returns>
        Task<int> RemoveExpiredTokensAsync(CancellationToken cancellationToken = default);
    }
}