using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using NomNomGo.IdentityService.Application.Interfaces.Services;

namespace NomNomGo.IdentityService.Infrastructure.Services
{
    /// <summary>
    /// Провайдер сервисных токенов для межсервисной коммуникации
    /// </summary>
    public class ServiceTokenProvider : IServiceTokenProvider
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ICacheService _cacheService;
        private readonly string _cacheKeyPrefix = "ServiceToken_";

        public ServiceTokenProvider(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ICacheService cacheService)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _cacheService = cacheService;
        }

        /// <summary>
        /// Получает сервисный токен для указанного целевого сервиса
        /// </summary>
        /// <param name="targetService">Имя целевого сервиса</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Сервисный токен</returns>
        public async Task<string> GetServiceTokenAsync(
            string targetService,
            CancellationToken cancellationToken = default)
        {
            // Пробуем получить токен из кэша
            var cacheKey = $"{_cacheKeyPrefix}{targetService}";
            var cachedToken = await _cacheService.GetAsync<TokenResponse>(cacheKey, cancellationToken);
            
            if (cachedToken != null && !string.IsNullOrEmpty(cachedToken.AccessToken))
            {
                // Проверяем, не истекает ли токен в ближайшее время
                var expirationTime = cachedToken.ExpirationTime;
                if (expirationTime > DateTime.UtcNow.AddMinutes(5))
                {
                    return cachedToken.AccessToken;
                }
            }

            // Получаем данные для аутентификации из конфигурации
            var identityServiceUrl = _configuration["Services:IdentityService:Url"];
            var clientId = _configuration[$"Services:{_configuration["ServiceName"]}:ClientId"];
            var clientSecret = _configuration[$"Services:{_configuration["ServiceName"]}:ClientSecret"];

            // Создаем HTTP-клиент
            var client = _httpClientFactory.CreateClient();

            // Формируем запрос для получения токена
            var request = new HttpRequestMessage(HttpMethod.Post, $"{identityServiceUrl}/api/token")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "client_credentials",
                    ["client_id"] = clientId,
                    ["client_secret"] = clientSecret,
                    ["scope"] = targetService
                })
            };

            // Отправляем запрос
            var response = await client.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            // Парсим ответ
            var content = await response.Content.ReadFromJsonAsync<TokenResponse>(
                cancellationToken: cancellationToken);
                
            if (content == null)
            {
                throw new InvalidOperationException("Не удалось получить токен");
            }

            // Вычисляем время истечения срока действия токена
            content.ExpirationTime = DateTime.UtcNow.AddSeconds(content.ExpiresIn);

            // Сохраняем токен в кэш
            await _cacheService.SetAsync(
                cacheKey,
                content,
                TimeSpan.FromSeconds(content.ExpiresIn),
                cancellationToken);

            return content.AccessToken;
        }

        /// <summary>
        /// Ответ с токеном
        /// </summary>
        private class TokenResponse
        {
            [JsonPropertyName("access_token")]
            public string AccessToken { get; set; } = string.Empty;
            
            [JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; }
            
            [JsonPropertyName("token_type")]
            public string TokenType { get; set; } = string.Empty;
            
            // Добавленное поле для хранения времени истечения срока действия токена
            public DateTime ExpirationTime { get; set; }
        }
    }
}