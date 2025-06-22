using NomNomGo.IdentityService.Application.DTOs.Response.Users;
using NomNomGo.IdentityService.Application.Interfaces.CQRS;
using NomNomGo.IdentityService.Application.Models;

namespace NomNomGo.IdentityService.Application.UseCases.Users.Queries.Get;

public record GetUserListQuery : IQuery<Result<PaginatedList<UserListItem>>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
    public bool? IsBlocked { get; init; }
    public string? Role { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; } = false;
}