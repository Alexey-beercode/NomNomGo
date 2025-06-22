using System.Security.Claims;
using NomNomGo.IdentityService.Application.Interfaces.Services;

namespace NomNomGo.IdentityService.API.Services
{
    /// <summary>
    /// Сервис для получения информации о текущем пользователе из HTTP контекста
    /// </summary>
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid UserId
        {
            get
            {
                var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                return !string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var id) ? id : Guid.Empty;
            }
        }

        public string Username => 
            _httpContextAccessor.HttpContext?.User?.FindFirstValue("username") ?? string.Empty;

        public IEnumerable<string> Roles => 
            _httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role)
                .Select(c => c.Value) ?? Array.Empty<string>();

        public IEnumerable<string> Permissions => 
            _httpContextAccessor.HttpContext?.User?.FindAll("permission")
                .Select(c => c.Value) ?? Array.Empty<string>();

        public bool IsAuthenticated => 
            _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

        public bool IsService => 
            _httpContextAccessor.HttpContext?.User?.HasClaim(c => c.Type == "client_id") ?? false;

        public string ServiceId => 
            _httpContextAccessor.HttpContext?.User?.FindFirstValue("client_id") ?? string.Empty;

        public IEnumerable<string> Scopes => 
            _httpContextAccessor.HttpContext?.User?.FindAll("scope")
                .Select(c => c.Value) ?? Array.Empty<string>();
    }
}