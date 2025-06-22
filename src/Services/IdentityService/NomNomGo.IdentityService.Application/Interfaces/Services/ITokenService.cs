using NomNomGo.IdentityService.Domain.Entities;

namespace NomNomGo.IdentityService.Application.Interfaces.Services
{
    /// <summary>
    /// Интерфейс сервиса для работы с токенами
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Генерирует JWT токен для пользователя
        /// </summary>
        /// <param name="user">Пользователь</param>
        /// <param name="roles">Роли пользователя</param>
        /// <param name="permissions">Разрешения пользователя</param>
        /// <returns>JWT токен</returns>
        string GenerateAccessToken(User user, IEnumerable<string> roles, IEnumerable<string> permissions);
        
        /// <summary>
        /// Генерирует токен обновления
        /// </summary>
        /// <returns>Токен обновления</returns>
        string GenerateRefreshToken();
        
        /// <summary>
        /// Генерирует сервисный токен для межсервисной коммуникации
        /// </summary>
        /// <param name="clientId">Идентификатор сервиса-клиента</param>
        /// <param name="scopes">Разрешенные области</param>
        /// <returns>JWT токен</returns>
        string GenerateServiceToken(string clientId, IEnumerable<string> scopes);
        
        /// <summary>
        /// Проверяет валидность токена
        /// </summary>
        /// <param name="token">Токен для проверки</param>
        /// <returns>Результат проверки</returns>
        bool ValidateToken(string token);
        
        /// <summary>
        /// Извлекает идентификатор пользователя из токена
        /// </summary>
        /// <param name="token">JWT токен</param>
        /// <returns>Идентификатор пользователя</returns>
        Guid? GetUserIdFromToken(string token);
        
        /// <summary>
        /// Извлекает идентификатор клиента из токена
        /// </summary>
        /// <param name="token">JWT токен</param>
        /// <returns>Идентификатор клиента</returns>
        string? GetClientIdFromToken(string token);

        int? GetTokenExpirationInMinutes();
    }
}