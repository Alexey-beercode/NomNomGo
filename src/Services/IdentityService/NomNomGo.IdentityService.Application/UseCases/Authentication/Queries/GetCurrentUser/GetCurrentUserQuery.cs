using NomNomGo.IdentityService.Application.DTOs.Response.Authentication;
using NomNomGo.IdentityService.Application.Interfaces.CQRS;
using NomNomGo.IdentityService.Application.Models;

namespace NomNomGo.IdentityService.Application.UseCases.Authentication.Queries.GetCurrentUser;

public record GetCurrentUserQuery : IQuery<Result<CurrentUserResponse>>
{
    // Токен будет извлечен из HTTP контекста
}