using MediatR;
using Microsoft.Extensions.Configuration;
using NomNomGo.IdentityService.Application.DTOs.Response.ServiceTokens;
using NomNomGo.IdentityService.Application.Interfaces.Services;
using NomNomGo.IdentityService.Application.Models;

namespace NomNomGo.IdentityService.Application.UseCases.ServiceTokens.IssueServiceToken.Commands
{
    public class IssueServiceTokenCommandHandler : IRequestHandler<IssueServiceTokenCommand, Result<ServiceTokenResponse>>
    {
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;

        public IssueServiceTokenCommandHandler(
            ITokenService tokenService,
            IConfiguration configuration)
        {
            _tokenService = tokenService;
            _configuration = configuration;
        }

        public async Task<Result<ServiceTokenResponse>> Handle(IssueServiceTokenCommand request, CancellationToken cancellationToken)
        {
            // Получение настроек сервисов из конфигурации
            var serviceSections = _configuration.GetSection("Services").GetChildren();
            
            // Проверка clientId
            var serviceSection = serviceSections.FirstOrDefault(s => 
                s.GetValue<string>("ClientId") == request.ClientId);
                
            if (serviceSection == null)
            {
                return Result<ServiceTokenResponse>.Failure("Неверный Client ID.");
            }

            // Проверка clientSecret
            var clientSecret = serviceSection.GetValue<string>("ClientSecret");
            if (clientSecret != request.ClientSecret)
            {
                return Result<ServiceTokenResponse>.Failure("Неверный Client Secret.");
            }

            // Проверка запрашиваемых областей (scopes)
            var serviceName = serviceSection.Key;
            var allowedScopes = GetAllowedScopesForService(serviceName);
            
            foreach (var scope in request.Scopes)
            {
                if (!allowedScopes.Contains(scope))
                {
                    return Result<ServiceTokenResponse>.Failure($"Область доступа '{scope}' не разрешена для сервиса '{serviceName}'.");
                }
            }

            // Генерация сервисного токена
            var accessToken = _tokenService.GenerateServiceToken(request.ClientId, request.Scopes);
            var expiresInMinutes = int.Parse(_configuration["Jwt:ServiceTokenExpirationInMinutes"]);
            var expiresIn = expiresInMinutes * 60; // в секундах
            var issuedAt = DateTime.UtcNow;
            var expiresAt = issuedAt.AddMinutes(expiresInMinutes);

            // Формирование ответа
            var response = new ServiceTokenResponse
            {
                AccessToken = accessToken,
                ExpiresIn = expiresIn,
                TokenType = "Bearer",
                Scope = string.Join(" ", request.Scopes),
            };

            return Result<ServiceTokenResponse>.Success(response);
        }

        private IEnumerable<string> GetAllowedScopesForService(string serviceName)
        {
            // В реальном приложении эта информация может быть в базе данных
            return serviceName.ToLower() switch
            {
                "identityservice" => new[] { "order-service", "restaurant-service", "tracking-service", "notification-service", "coupon-service" },
                "orderservice" => new[] { "identity-service", "restaurant-service", "tracking-service", "notification-service", "coupon-service" },
                "restaurantservice" => new[] { "identity-service", "order-service" },
                "trackingservice" => new[] { "identity-service", "order-service" },
                "notificationservice" => new[] { "identity-service", "order-service" },
                "couponservice" => new[] { "identity-service", "order-service" },
                _ => Array.Empty<string>()
            };
        }
    }
}