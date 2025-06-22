using NomNomGo.IdentityService.Application.DTOs.Response.ServiceTokens;
using NomNomGo.IdentityService.Application.Interfaces.CQRS;
using NomNomGo.IdentityService.Application.Models;

namespace NomNomGo.IdentityService.Application.UseCases.ServiceTokens.IssueServiceToken.Commands;

public record IssueServiceTokenCommand : ICommand<Result<ServiceTokenResponse>>
{
    public string ClientId { get; init; } = string.Empty;
    public string ClientSecret { get; init; } = string.Empty;
    public IEnumerable<string> Scopes { get; init; } = new List<string>();
}