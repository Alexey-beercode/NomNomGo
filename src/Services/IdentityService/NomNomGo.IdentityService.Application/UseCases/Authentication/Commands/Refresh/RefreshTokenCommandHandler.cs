using MediatR;
using NomNomGo.IdentityService.Application.DTOs.Response.Authentication;
using NomNomGo.IdentityService.Application.Interfaces.Services;
using NomNomGo.IdentityService.Application.Models;
using NomNomGo.IdentityService.Domain.Entities;
using NomNomGo.IdentityService.Domain.Entities.Relationships;
using NomNomGo.IdentityService.Domain.Exceptions;
using NomNomGo.IdentityService.Domain.Interfaces.UnitOfWork;

namespace NomNomGo.IdentityService.Application.UseCases.Authentication.Commands.Refresh
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;

        public RefreshTokenCommandHandler(
            IUnitOfWork unitOfWork,
            ITokenService tokenService)
        {
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
        }

        public async Task<Result<RefreshTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            // Поиск refresh токена
            var refreshToken = await _unitOfWork.RefreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);
            if (refreshToken == null)
            {
                throw new AuthenticationException("Недействительный refresh токен.");
            }

            // Проверка срока действия токена
            if (refreshToken.ExpiresAt < DateTime.UtcNow)
            {
                // Удаление просроченного токена
                _unitOfWork.RefreshTokenRepository.Delete(refreshToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                
                throw new AuthenticationException("Срок действия refresh токена истек.");
            }

            // Получение пользователя
            var user = await _unitOfWork.UserRepository.GetByIdAsync(refreshToken.UserId, cancellationToken);
            if (user == null)
            {
                throw new NotFoundException("Пользователь не найден.");
            }

            // Проверка блокировки пользователя
            if (user.IsBlocked)
            {
                if (user.BlockedUntil.HasValue && user.BlockedUntil.Value < DateTime.UtcNow)
                {
                    // Срок блокировки истек, разблокируем пользователя
                    user.IsBlocked = false;
                    user.BlockedUntil = null;
                    _unitOfWork.UserRepository.Update(user);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
                else
                {
                    var blockMessage = user.BlockedUntil.HasValue
                        ? $"Ваша учетная запись заблокирована до {user.BlockedUntil.Value:g}."
                        : "Ваша учетная запись заблокирована.";
                    
                    throw new AuthenticationException(blockMessage);
                }
            }

            // Получение ролей пользователя
            var userWithRoles = await _unitOfWork.UserRepository.GetWithRolesAsync(user.Id, cancellationToken);
            var roleNames = userWithRoles?.UserRoles
                .Select(ur => ur.Role.Name)
                .ToList() ?? new List<string>();

            // Получение разрешений пользователя
            var permissions = new List<string>();
            foreach (var userRole in userWithRoles?.UserRoles ?? Enumerable.Empty<UserRole>())
            {
                var role = await _unitOfWork.RoleRepository.GetWithPermissionsAsync(userRole.RoleId, cancellationToken);
                if (role != null)
                {
                    var rolePermissions = role.RolePermissions
                        .Select(rp => rp.Permission.Name);
                    
                    permissions.AddRange(rolePermissions);
                }
            }

            // Создание новых токенов
            var newAccessToken = _tokenService.GenerateAccessToken(user, roleNames, permissions.Distinct());
            var newRefreshToken = _tokenService.GenerateRefreshToken();
            var expiresAt = DateTime.UtcNow.AddMinutes((double)(_tokenService.GetTokenExpirationInMinutes()!));

            // Удаление старого refresh токена
            _unitOfWork.RefreshTokenRepository.Delete(refreshToken);

            // Сохранение нового refresh токена
            var newRefreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = newRefreshToken,
                ExpiresAt = expiresAt.AddDays(7) // Refresh токен действует дольше access токена
            };

            await _unitOfWork.RefreshTokenRepository.AddAsync(newRefreshTokenEntity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Формирование ответа
            var response = new RefreshTokenResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = expiresAt
            };

            return Result<RefreshTokenResponse>.Success(response);
        }
    }
}