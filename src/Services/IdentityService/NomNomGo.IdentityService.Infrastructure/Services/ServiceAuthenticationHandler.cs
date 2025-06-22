using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using NomNomGo.IdentityService.Application.Interfaces.Services;
using NomNomGo.IdentityService.Domain.Interfaces;

namespace NomNomGo.IdentityService.Infrastructure.Services
{
    /// <summary>
    /// Обработчик аутентификации для межсервисных запросов
    /// </summary>
    public class ServiceAuthenticationHandler
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IServiceTokenProvider _tokenProvider;

        public ServiceAuthenticationHandler(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            IServiceTokenProvider tokenProvider)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _tokenProvider = tokenProvider;
        }

        /// <summary>
        /// Отправляет аутентифицированный запрос к другому сервису
        /// </summary>
        /// <param name="serviceName">Имя сервиса</param>
        /// <param name="method">HTTP-метод</param>
        /// <param name="endpoint">Конечная точка</param>
        /// <param name="content">Содержимое запроса</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Ответ HTTP</returns>
        public async Task<HttpResponseMessage> SendAuthenticatedRequestAsync(
            string serviceName,
            HttpMethod method,
            string endpoint,
            object? content = null,
            CancellationToken cancellationToken = default)
        {
            // Получение URL сервиса из конфигурации
            var serviceUrl = _configuration[$"Services:{serviceName}:Url"];
            
            // Получение сервисного токена
            var token = await _tokenProvider.GetServiceTokenAsync(serviceName, cancellationToken);
            
            // Создание HTTP-клиента
            var client = _httpClientFactory.CreateClient();
            
            // Формирование запроса
            var request = new HttpRequestMessage(method, $"{serviceUrl}/{endpoint}");
            
            // Добавление токена в заголовок
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            // Добавление содержимого, если есть
            if (content != null)
            {
                request.Content = JsonContent.Create(content);
            }
            
            // Отправка запроса
            return await client.SendAsync(request, cancellationToken);
        }
    }
}