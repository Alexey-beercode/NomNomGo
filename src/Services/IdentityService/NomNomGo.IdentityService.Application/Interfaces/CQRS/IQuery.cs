using MediatR;

namespace NomNomGo.IdentityService.Application.Interfaces.CQRS;

public interface IQuery : IRequest<Unit> { }

public interface IQuery<out TResult> : IRequest<TResult> { }
