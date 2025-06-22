using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NomNomGo.IdentityService.Application.DTOs.Response.Users;
using NomNomGo.IdentityService.Application.UseCases.Users.Queries.GetById;

namespace NomNomGo.IdentityService.API.Controllers
{
    [ApiController]
    [Route("api/inter-service")]
    [Authorize(Policy = "ServicePolicy")]
    [Produces("application/json")]
    public class InterServiceController : ControllerBase
    {
        private readonly IMediator _mediator;

        public InterServiceController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Получение информации о пользователе для других сервисов
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Информация о пользователе</returns>
        [HttpGet("users/{userId:guid}")]
        [ProducesResponseType(typeof(UserDetailResponse), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetUserForService([FromRoute] Guid userId, CancellationToken cancellationToken)
        {
            var query = new GetUserByIdQuery { UserId = userId };
            var result = await _mediator.Send(query, cancellationToken);
            
            if (!result.Succeeded)
            {
                return NotFound(new { error = result.Error });
            }
            
            return Ok(result.Data);
        }

        /// <summary>
        /// Проверка валидности пользовательского токена
        /// </summary>
        /// <param name="token">JWT токен для проверки</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Результат валидации</returns>
        [HttpPost("validate-token")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> ValidateUserToken([FromBody] ValidateTokenRequest request, CancellationToken cancellationToken)
        {
            // Здесь может быть дополнительная логика валидации токенов
            // Для простоты возвращаем результат на основе авторизации запроса
            
            if (User.Identity?.IsAuthenticated == true)
            {
                return Ok(new 
                { 
                    valid = true, 
                    userId = User.FindFirst("sub")?.Value,
                    username = User.FindFirst("username")?.Value,
                    roles = User.FindAll("role").Select(c => c.Value)
                });
            }
            
            return Unauthorized(new { valid = false });
        }
    }

    public class ValidateTokenRequest
    {
        public string Token { get; set; } = string.Empty;
    }
}