using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using NomNomGo.IdentityService.Application.Interfaces.Services;

namespace NomNomGo.IdentityService.Infrastructure.Services
{
    /// <summary>
    /// Сервис для кэширования
    /// </summary>
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _cache;

        public CacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        /// <summary>
        /// Получает данные из кэша
        /// </summary>
        /// <typeparam name="T">Тип данных</typeparam>
        /// <param name="key">Ключ</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Данные или null, если ключ не найден</returns>
        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            var cachedData = await _cache.GetStringAsync(key, cancellationToken);
            
            if (string.IsNullOrEmpty(cachedData))
            {
                return default;
            }
            
            return JsonSerializer.Deserialize<T>(cachedData);
        }

        /// <summary>
        /// Сохраняет данные в кэш
        /// </summary>
        /// <typeparam name="T">Тип данных</typeparam>
        /// <param name="key">Ключ</param>
        /// <param name="value">Значение</param>
        /// <param name="expirationTime">Время истечения срока действия</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Задача, представляющая асинхронную операцию</returns>
        public async Task SetAsync<T>(
            string key, 
            T value, 
            TimeSpan? expirationTime = null, 
            CancellationToken cancellationToken = default)
        {
            var options = new DistributedCacheEntryOptions();
            
            if (expirationTime.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = expirationTime;
            }
            
            var serializedData = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, serializedData, options, cancellationToken);
        }

        /// <summary>
        /// Удаляет данные из кэша
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Задача, представляющая асинхронную операцию</returns>
        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            await _cache.RemoveAsync(key, cancellationToken);
        }
    }
}