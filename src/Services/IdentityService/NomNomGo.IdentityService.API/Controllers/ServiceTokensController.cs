using MediatR;
using Microsoft.AspNetCore.Mvc;
using NomNomGo.IdentityService.Application.DTOs.Response.ServiceTokens;
using NomNomGo.IdentityService.Application.UseCases.ServiceTokens.IssueServiceToken.Commands;

namespace NomNomGo.IdentityService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ServiceTokensController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ServiceTokensController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Выдача сервисного токена для межсервисной коммуникации
        /// </summary>
        /// <param name="command">Данные для получения сервисного токена</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Сервисный токен</returns>
        [HttpPost("token")]
        [ProducesResponseType(typeof(ServiceTokenResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> IssueServiceToken([FromBody] IssueServiceTokenCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            
            if (!result.Succeeded)
            {
                return Unauthorized(new { error = result.Error });
            }
            
            return Ok(result.Data);
        }
    }
}