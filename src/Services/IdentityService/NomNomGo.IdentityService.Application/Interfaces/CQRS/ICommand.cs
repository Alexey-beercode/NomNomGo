using MediatR;

namespace NomNomGo.IdentityService.Application.Interfaces.CQRS;

public interface ICommand : IRequest<Unit> { }
    
public interface ICommand<out TResult> : IRequest<TResult> { }