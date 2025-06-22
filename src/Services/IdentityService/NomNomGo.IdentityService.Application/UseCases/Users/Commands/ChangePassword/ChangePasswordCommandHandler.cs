using NomNomGo.IdentityService.Application.Interfaces.CQRS;
using NomNomGo.IdentityService.Application.Models;
using NomNomGo.IdentityService.Domain.Interfaces.UnitOfWork;
using System.Security.Cryptography;
using System.Text;
using MediatR;
using NomNomGo.IdentityService.Application.Interfaces.Services;

namespace NomNomGo.IdentityService.Application.UseCases.Users.Commands.ChangePassword
{
    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public ChangePasswordCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (userId == Guid.Empty)
            {
                return Result.Failure("Пользователь не аутентифицирован.");
            }

            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return Result.Failure("Пользователь не найден.");
            }

            // Проверка блокировки пользователя
            if (user.IsBlocked)
            {
                var blockMessage = user.BlockedUntil.HasValue
                    ? $"Ваша учетная запись заблокирована до {user.BlockedUntil.Value:g}."
                    : "Ваша учетная запись заблокирована.";
                
                return Result.Failure(blockMessage);
            }

            // Проверка текущего пароля
            if (!VerifyPasswordHash(request.CurrentPassword, user.PasswordHash))
            {
                return Result.Failure("Неверный текущий пароль.");
            }

            // Хэширование нового пароля
            var (passwordHash, salt) = CreatePasswordHash(request.NewPassword);
            var hashedPassword = $"{passwordHash}.{Convert.ToBase64String(salt)}";

            // Обновление пароля
            user.PasswordHash = hashedPassword;
            user.UpdatedAt = DateTime.UtcNow;

            // Сохранение изменений
            _unitOfWork.UserRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Удаление всех refresh токенов пользователя для повторной аутентификации
            var refreshTokens = await _unitOfWork.RefreshTokenRepository.GetActiveByUserIdAsync(userId, cancellationToken);
            foreach (var token in refreshTokens)
            {
                _unitOfWork.RefreshTokenRepository.Delete(token);
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }

        private bool VerifyPasswordHash(string password, string storedHash)
        {
            var parts = storedHash.Split('.');
            if (parts.Length != 2)
            {
                return false;
            }

            var storedHashValue = parts[0];
            var storedSalt = Convert.FromBase64String(parts[1]);

            using var hmac = new HMACSHA512(storedSalt);
            var computedHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));

            return computedHash == storedHashValue;
        }

        private (string hash, byte[] salt) CreatePasswordHash(string password)
        {
            using var hmac = new HMACSHA512();
            var salt = hmac.Key;
            var hash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
            
            return (hash, salt);
        }
    }
}