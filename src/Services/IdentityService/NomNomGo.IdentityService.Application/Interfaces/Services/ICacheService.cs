namespace NomNomGo.IdentityService.Application.Interfaces.Services
{
    /// <summary>
    /// Интерфейс сервиса для кэширования
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Получает данные из кэша
        /// </summary>
        /// <typeparam name="T">Тип данных</typeparam>
        /// <param name="key">Ключ</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Данные или null, если ключ не найден</returns>
        Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Сохраняет данные в кэш
        /// </summary>
        /// <typeparam name="T">Тип данных</typeparam>
        /// <param name="key">Ключ</param>
        /// <param name="value">Значение</param>
        /// <param name="expirationTime">Время истечения срока действия</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Задача, представляющая асинхронную операцию</returns>
        Task SetAsync<T>(
            string key, 
            T value, 
            TimeSpan? expirationTime = null, 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Удаляет данные из кэша
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Задача, представляющая асинхронную операцию</returns>
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    }
}