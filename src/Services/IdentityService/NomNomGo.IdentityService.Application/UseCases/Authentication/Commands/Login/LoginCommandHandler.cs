using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NomNomGo.IdentityService.Application.DTOs.Response.Authentication;
using NomNomGo.IdentityService.Application.Interfaces.Services;
using NomNomGo.IdentityService.Application.Models;
using NomNomGo.IdentityService.Domain.Entities;
using NomNomGo.IdentityService.Domain.Entities.Relationships;
using NomNomGo.IdentityService.Domain.Exceptions;
using NomNomGo.IdentityService.Domain.Interfaces.UnitOfWork;

namespace NomNomGo.IdentityService.Application.UseCases.Authentication.Commands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly ILogger<LoginCommandHandler> _logger;

        public LoginCommandHandler(
            IUnitOfWork unitOfWork,
            ITokenService tokenService,
            IMapper mapper, ILogger<LoginCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation(request.Login + " " + request.Password);
            // Поиск пользователя по email или username
            var user = await _unitOfWork.UserRepository.GetByEmailAsync(request.Login, cancellationToken) ??
                       await _unitOfWork.UserRepository.GetByUsernameAsync(request.Login, cancellationToken);

            _logger.LogInformation((user == null).ToString());
            if (user == null)
            {
                throw new AuthenticationException("Неверные учетные данные.");
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

            // Проверка пароля
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                throw new AuthenticationException("Неверный пароль");
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

            // Создание токенов
            var accessToken = _tokenService.GenerateAccessToken(user, roleNames, permissions.Distinct());
            var refreshToken = _tokenService.GenerateRefreshToken();
            var expiresAt = DateTime.UtcNow.AddMinutes((double)(_tokenService.GetTokenExpirationInMinutes()!));

            // Сохранение refresh токена
            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiresAt = expiresAt.AddDays(7) // Refresh токен действует дольше access токена
            };

            await _unitOfWork.RefreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Формирование ответа
            var response = new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Username = user.Username,
                Roles = roleNames,
                ExpiresAt = expiresAt
            };

            return Result<LoginResponse>.Success(response);
        }
    }
}