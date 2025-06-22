using MediatR;
using NomNomGo.IdentityService.Application.Models;
using NomNomGo.IdentityService.Domain.Interfaces.UnitOfWork;

namespace NomNomGo.IdentityService.Application.UseCases.Authentication.Commands.Logout
{
    public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;

        public LogoutCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            // Поиск refresh токена
            var refreshToken = await _unitOfWork.RefreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);
            
            // Если токен найден, удаляем его
            if (refreshToken != null)
            {
                _unitOfWork.RefreshTokenRepository.Delete(refreshToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            // Всегда возвращаем успешный результат, даже если токен не найден,
            // чтобы не раскрывать информацию о существовании/валидности токенов
            return Result.Success();
        }
    }
}