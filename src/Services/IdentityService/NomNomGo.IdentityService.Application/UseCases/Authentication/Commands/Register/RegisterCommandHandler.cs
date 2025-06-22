using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using NomNomGo.IdentityService.Application.DTOs.Response.Authentication;
using NomNomGo.IdentityService.Application.Interfaces.Services;
using NomNomGo.IdentityService.Application.Models;
using NomNomGo.IdentityService.Domain.Entities;
using NomNomGo.IdentityService.Domain.Entities.Relationships;
using NomNomGo.IdentityService.Domain.Exceptions;
using NomNomGo.IdentityService.Domain.Interfaces.UnitOfWork;
using NomNomGo.IdentityService.Domain.Models;

namespace NomNomGo.IdentityService.Application.UseCases.Authentication.Commands.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<RegisterResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly ILogger<RegisterCommandHandler> _logger;
        private readonly IEmailService _emailService;

        public RegisterCommandHandler(
            IUnitOfWork unitOfWork,
            ITokenService tokenService,
            IMapper mapper,
            ILogger<RegisterCommandHandler> logger,
            IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _mapper = mapper;
            _logger = logger;
            _emailService = emailService;
        }

        public async Task<Result<RegisterResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Начало регистрации пользователя с email: {Email}", request.Email);

                // Проверка на уникальность email
                var existingUserByEmail = await _unitOfWork.UserRepository.GetByEmailAsync(request.Email, cancellationToken);
                if (existingUserByEmail != null)
                {
                    _logger.LogWarning("Попытка регистрации с существующим email: {Email}", request.Email);
                    
                    var emailErrors = new List<ValidationFailure>
                    {
                        new ValidationFailure("Email", "Пользователь с таким email уже существует")
                    };
                    throw new ValidationException(emailErrors);
                }

                // Проверка на уникальность username
                var existingUserByUsername = await _unitOfWork.UserRepository.GetByUsernameAsync(request.Username, cancellationToken);
                if (existingUserByUsername != null)
                {
                    _logger.LogWarning("Попытка регистрации с существующим username: {Username}", request.Username);
                    
                    var usernameErrors = new List<ValidationFailure>
                    {
                        new ValidationFailure("Username", "Пользователь с таким именем уже существует")
                    };
                    throw new ValidationException(usernameErrors);
                }

                // Хэширование пароля
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                var hashedPassword = passwordHash;

                // Создание пользователя
                var user = new User
                {
                    Email = request.Email,
                    Username = request.Username,
                    PasswordHash = hashedPassword,
                    PhoneNumber = request.PhoneNumber,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _unitOfWork.UserRepository.AddAsync(user, cancellationToken);
                
                // Первое сохранение для получения ID пользователя
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                
                _logger.LogInformation("Пользователь создан с ID: {UserId}", user.Id);

                var roleName = (bool)request.IsCourier ? "Courier" : "User";
                var defaultRole = await _unitOfWork.RoleRepository.GetByNameAsync(roleName, cancellationToken);

                if (defaultRole == null)
                {
                    _logger.LogInformation("Создание роли '{RoleName}' по умолчанию", roleName);
                    defaultRole = new Role
                    {
                        Name = roleName,
                    };
                    await _unitOfWork.RoleRepository.AddAsync(defaultRole, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }

                // Создание связи пользователь-роль
                var userRole = new UserRole
                {
                    UserId = user.Id,
                    RoleId = defaultRole.Id
                };

                await _unitOfWork.UserRoleRepository.AddAsync(userRole, cancellationToken);

                // Создание токенов
                var roleNames = new List<string> { roleName };
                var accessToken = _tokenService.GenerateAccessToken(user, roleNames, Array.Empty<string>());
                var refreshToken = _tokenService.GenerateRefreshToken();
                var tokenExpirationMinutes = _tokenService.GetTokenExpirationInMinutes() ?? 60;
                var expiresAt = DateTime.UtcNow.AddMinutes(tokenExpirationMinutes);

                // Сохранение refresh токена
                var refreshTokenEntity = new RefreshToken
                {
                    UserId = user.Id,
                    Token = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddDays(7), // Refresh token живет 7 дней
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.RefreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);
                
                // Финальное сохранение всех изменений
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Пользователь {UserId} успешно зарегистрирован", user.Id);

                // Формирование ответа
                var response = new RegisterResponse
                {
                    UserId = user.Id,
                    Username = user.Username,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = expiresAt
                };

                // Отправка email уведомления (не блокирующая операция)
                _ = Task.Run(async () => await SendRegistrationEmailAsync(user), cancellationToken);

                return Result<RegisterResponse>.Success(response);
            }
            catch (ValidationException)
            {
                // ValidationException пробрасываем дальше без логирования как ошибку
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при регистрации пользователя с email: {Email}", request.Email);
                return Result<RegisterResponse>.Failure("Произошла ошибка при регистрации пользователя");
            }
        }

        private async Task SendRegistrationEmailAsync(User user)
        {
            try
            {
                _logger.LogInformation("Отправка email уведомления для пользователя: {Email}", user.Email);
                
                var registrationEmail = new RegistrationEmail
                {
                    ToEmail = user.Email,
                    Name = user.Username,
                    AppName = "NomNomGo"
                };

                await _emailService.SendRegistrationEmailAsync(registrationEmail);
                
                _logger.LogInformation("Email уведомление успешно отправлено для: {Email}", user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отправке email уведомления для: {Email}", user.Email);
            }
        }
    }
}