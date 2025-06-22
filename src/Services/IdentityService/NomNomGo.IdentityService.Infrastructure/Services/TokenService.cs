using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NomNomGo.IdentityService.Application.Interfaces.Services;
using NomNomGo.IdentityService.Domain.Entities;

namespace NomNomGo.IdentityService.Infrastructure.Services
{
    /// <summary>
    /// Сервис для работы с токенами
    /// </summary>
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly ICacheService _cacheService;

        public TokenService(IConfiguration configuration, ICacheService cacheService)
        {
            _configuration = configuration;
            _cacheService = cacheService;
        }

        /// <summary>
        /// Генерирует JWT токен для пользователя
        /// </summary>
        /// <param name="user">Пользователь</param>
        /// <param name="roles">Роли пользователя</param>
        /// <param name="permissions">Разрешения пользователя</param>
        /// <returns>JWT токен</returns>
        public string GenerateAccessToken(User user, IEnumerable<string> roles, IEnumerable<string> permissions)
        {
            var signingKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
            
            var signingCredentials = new SigningCredentials(
                signingKey, SecurityAlgorithms.HmacSha256);
            
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new("username", user.Username)
            };
            
            // Добавление ролей
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            
            // Добавление разрешений
            foreach (var permission in permissions)
            {
                claims.Add(new Claim("permission", permission));
            }
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(
                    double.Parse(_configuration["Jwt:ExpirationInMinutes"])),
                SigningCredentials = signingCredentials,
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Генерирует токен обновления
        /// </summary>
        /// <returns>Токен обновления</returns>
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        /// <summary>
        /// Генерирует сервисный токен для межсервисной коммуникации
        /// </summary>
        /// <param name="clientId">Идентификатор сервиса-клиента</param>
        /// <param name="scopes">Разрешенные области</param>
        /// <returns>JWT токен</returns>
        public string GenerateServiceToken(string clientId, IEnumerable<string> scopes)
        {
            var signingKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
            
            var signingCredentials = new SigningCredentials(
                signingKey, SecurityAlgorithms.HmacSha256);
            
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, clientId),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new("client_id", clientId),
                new(ClaimTypes.Role, "service"),

            };
            
            // Добавление областей (scopes)
            foreach (var scope in scopes)
            {
                claims.Add(new Claim("scope", scope));
            }
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(
                    double.Parse(_configuration["Jwt:ServiceTokenExpirationInMinutes"])),
                SigningCredentials = signingCredentials,
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Проверяет валидность токена
        /// </summary>
        /// <param name="token">Токен для проверки</param>
        /// <returns>Результат проверки</returns>
        public bool ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = GetValidationParameters();
            
            try
            {
                tokenHandler.ValidateToken(token, validationParameters, out _);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Извлекает идентификатор пользователя из токена
        /// </summary>
        /// <param name="token">JWT токен</param>
        /// <returns>Идентификатор пользователя</returns>
        public Guid? GetUserIdFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = GetValidationParameters();
            
            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                var subClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub);
                
                if (subClaim != null && Guid.TryParse(subClaim.Value, out var userId))
                {
                    return userId;
                }
                
                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Извлекает идентификатор клиента из токена
        /// </summary>
        /// <param name="token">JWT токен</param>
        /// <returns>Идентификатор клиента</returns>
        public string? GetClientIdFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = GetValidationParameters();
            
            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                return principal.FindFirst("client_id")?.Value;
            }
            catch
            {
                return null;
            }
        }

        public int? GetTokenExpirationInMinutes()
        {
            return int.Parse(_configuration["Jwt:ExpirationInMinutes"]);
        }

        /// <summary>
        /// Получает параметры валидации токена
        /// </summary>
        /// <returns>Параметры валидации</returns>
        private TokenValidationParameters GetValidationParameters()
        {
            return new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]))
            };
        }
    }
}