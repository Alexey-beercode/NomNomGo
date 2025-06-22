using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NomNomGo.IdentityService.Application.DTOs.Request;
using NomNomGo.IdentityService.Application.DTOs.Response.Users;
using NomNomGo.IdentityService.Application.Models;
using NomNomGo.IdentityService.Application.UseCases.Users.Commands.BlockUser;
using NomNomGo.IdentityService.Application.UseCases.Users.Commands.UpdateProfile;
using NomNomGo.IdentityService.Application.UseCases.Users.Commands.ChangePassword;
using NomNomGo.IdentityService.Application.UseCases.Users.Commands.UnblockUser;
using NomNomGo.IdentityService.Application.UseCases.Users.Queries.Get;
using NomNomGo.IdentityService.Application.UseCases.Users.Queries.GetById;

namespace NomNomGo.IdentityService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Получение списка пользователей
        /// </summary>
        /// <param name="query">Параметры запроса</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Пагинированный список пользователей</returns>
        [HttpGet]
        [Authorize(Roles = "Admin,Moderator")]
        [ProducesResponseType(typeof(PaginatedList<UserListItem>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetUsers([FromQuery] GetUserListQuery query,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(query, cancellationToken);

            if (!result.Succeeded)
            {
                return BadRequest(new { error = result.Error });
            }

            return Ok(result.Data);
        }

        /// <summary>
        /// Получение пользователя по ID
        /// </summary>
        /// <param name="id">Идентификатор пользователя</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Детальная информация о пользователе</returns>
        [HttpGet("{id:guid}")]
        [Authorize(Roles = "Admin,Moderator")]
        [ProducesResponseType(typeof(UserDetailResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetUser([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var query = new GetUserByIdQuery { UserId = id };
            var result = await _mediator.Send(query, cancellationToken);

            if (!result.Succeeded)
            {
                return NotFound(new { error = result.Error });
            }

            return Ok(result.Data);
        }

        /// <summary>
        /// Обновление профиля текущего пользователя
        /// </summary>
        /// <param name="command">Данные для обновления</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Обновленная информация о пользователе</returns>
        [HttpPut("profile")]
        [ProducesResponseType(typeof(UpdateProfileResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileCommand command,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);

            if (!result.Succeeded)
            {
                return BadRequest(new { error = result.Error });
            }

            return Ok(result.Data);
        }

        /// <summary>
        /// Изменение пароля текущего пользователя
        /// </summary>
        /// <param name="command">Данные для смены пароля</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Статус операции</returns>
        [HttpPut("password")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);

            if (!result.Succeeded)
            {
                return BadRequest(new { error = result.Error });
            }

            return Ok(new { message = "Пароль успешно изменен" });
        }

        /// <summary>
        /// Блокировка пользователя
        /// </summary>
        /// <param name="id">Идентификатор пользователя</param>
        /// <param name="request">Данные для блокировки</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Обновленная информация о пользователе</returns>
        [HttpPut("{id:guid}/block")]
        [Authorize(Roles = "Admin,Moderator")]
        [ProducesResponseType(typeof(UserDetailResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> BlockUser([FromRoute] Guid id, [FromBody] BlockUserRequest request,
            CancellationToken cancellationToken)
        {
            var command = new BlockUserCommand
            {
                UserId = id,
                Reason = request.Reason,
                Duration = request.Duration
            };

            var result = await _mediator.Send(command, cancellationToken);

            if (!result.Succeeded)
            {
                return BadRequest(new { error = result.Error });
            }

            return Ok(result.Data);
        }

        /// <summary>
        /// Разблокировка пользователя
        /// </summary>
        /// <param name="id">Идентификатор пользователя</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Обновленная информация о пользователе</returns>
        [HttpPut("{id:guid}/unblock")]
        [Authorize(Roles = "Admin,Moderator")]
        [ProducesResponseType(typeof(UserDetailResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UnblockUser([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var command = new UnblockUserCommand { UserId = id };

            var result = await _mediator.Send(command, cancellationToken);

            if (!result.Succeeded)
            {
                return BadRequest(new { error = result.Error });
            }

            return Ok(result.Data);
        }
    }
}