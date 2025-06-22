namespace NomNomGo.IdentityService.Application.Interfaces.Services
{
    /// <summary>
    /// Интерфейс провайдера сервисных токенов для межсервисной коммуникации
    /// </summary>
    public interface IServiceTokenProvider
    {
        /// <summary>
        /// Получает сервисный токен для указанного целевого сервиса
        /// </summary>
        /// <param name="targetService">Имя целевого сервиса</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Сервисный токен</returns>
        Task<string> GetServiceTokenAsync(
            string targetService, 
            CancellationToken cancellationToken = default);
    }
}