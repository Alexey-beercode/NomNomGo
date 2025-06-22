using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using NomNomGo.IdentityService.Application.DTOs.Response.Authentication;
using NomNomGo.IdentityService.Application.UseCases.Authentication.Commands.Login;
using NomNomGo.IdentityService.Application.UseCases.Authentication.Commands.Register;
using NomNomGo.IdentityService.Application.UseCases.Authentication.Commands.Logout;
using NomNomGo.IdentityService.Application.UseCases.Authentication.Commands.Refresh;
using NomNomGo.IdentityService.Application.UseCases.Authentication.Queries.GetCurrentUser;

namespace NomNomGo.IdentityService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthenticationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Аутентификация пользователя
        /// </summary>
        /// <param name="command">Данные для входа</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Токены доступа</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            
            if (!result.Succeeded)
            {
                return BadRequest(new { error = result.Error });
            }
            
            return Ok(result.Data);
        }

        /// <summary>
        /// Регистрация нового пользователя
        /// </summary>
        /// <param name="command">Данные для регистрации</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Информация о зарегистрированном пользователе и токены</returns>
        [HttpPost("register")]
        [ProducesResponseType(typeof(RegisterResponse), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            
            if (!result.Succeeded)
            {
                return BadRequest(new { error = result.Error});
            }
            
            return CreatedAtAction(nameof(GetCurrentUser), result.Data);
        }

        /// <summary>
        /// Обновление токена доступа
        /// </summary>
        /// <param name="command">Refresh токен</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Новые токены доступа</returns>
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(RefreshTokenResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            
            if (!result.Succeeded)
            {
                return Unauthorized(new { error = result.Error });
            }
            
            return Ok(result.Data);
        }

        /// <summary>
        /// Выход из системы
        /// </summary>
        /// <param name="command">Refresh токен для удаления</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Статус операции</returns>
        [HttpPost("logout")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Logout([FromBody] LogoutCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            
            if (!result.Succeeded)
            {
                return BadRequest(new { error = result.Error});
            }
            
            return Ok(new { message = "Успешный выход из системы" });
        }

        /// <summary>
        /// Получение информации о текущем пользователе
        /// </summary>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Информация о текущем пользователе</returns>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(CurrentUserResponse), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
        {
            var query = new GetCurrentUserQuery();
            var result = await _mediator.Send(query, cancellationToken);
            
            if (!result.Succeeded)
            {
                return Unauthorized(new { error = result.Error });
            }
            
            return Ok(result.Data);
        }
    }
}