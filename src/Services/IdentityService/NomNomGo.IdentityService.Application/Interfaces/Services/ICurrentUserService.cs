namespace NomNomGo.IdentityService.Application.Interfaces.Services
{
    /// <summary>
    /// Сервис для получения информации о текущем пользователе
    /// </summary>
    public interface ICurrentUserService
    {
        /// <summary>
        /// Идентификатор текущего пользователя
        /// </summary>
        Guid UserId { get; }
        
        /// <summary>
        /// Имя пользователя
        /// </summary>
        string Username { get; }
        
        /// <summary>
        /// Роли пользователя
        /// </summary>
        IEnumerable<string> Roles { get; }
        
        /// <summary>
        /// Разрешения пользователя
        /// </summary>
        IEnumerable<string> Permissions { get; }
        
        /// <summary>
        /// Признак аутентифицированного пользователя
        /// </summary>
        bool IsAuthenticated { get; }
        
        /// <summary>
        /// Признак сервисного запроса
        /// </summary>
        bool IsService { get; }
        
        /// <summary>
        /// Идентификатор сервиса (для сервисных запросов)
        /// </summary>
        string ServiceId { get; }
        
        /// <summary>
        /// Области доступа сервиса (для сервисных запросов)
        /// </summary>
        IEnumerable<string> Scopes { get; }
    }
}