using AutoMapper;
using MediatR;
using NomNomGo.IdentityService.Application.DTOs.Response.Users;
using NomNomGo.IdentityService.Application.Models;
using NomNomGo.IdentityService.Domain.Interfaces.UnitOfWork;

namespace NomNomGo.IdentityService.Application.UseCases.Users.Commands.BlockUser;

public class BlockUserCommandHandler : IRequestHandler<BlockUserCommand, Result<UserDetailResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public BlockUserCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<UserDetailResponse>> Handle(BlockUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _unitOfWork.UserRepository.GetWithRolesAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                return Result<UserDetailResponse>.Failure("Пользователь не найден.");
            }

            // Проверяем, не заблокирован ли уже пользователь
            if (user.IsBlocked && user.BlockedUntil.HasValue && user.BlockedUntil > DateTime.UtcNow)
            {
                return Result<UserDetailResponse>.Failure("Пользователь уже заблокирован.");
            }

            // Определяем дату окончания блокировки
            var blockedUntil = CalculateBlockedUntil(request.Duration);

            // Блокируем пользователя
            user.IsBlocked = true;
            user.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.UserRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Создаем ответ
            var response = new UserDetailResponse
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                IsBlocked = user.IsBlocked,
                BlockedUntil = user.BlockedUntil,
                Roles = user.UserRoles?.Select(ur => ur.Role.Name).ToList() ?? new List<string>(),
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };

            return Result<UserDetailResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<UserDetailResponse>.Failure($"Произошла ошибка при блокировке пользователя: {ex.Message}");
        }
    }

    private static DateTime? CalculateBlockedUntil(string? duration)
    {
        if (string.IsNullOrEmpty(duration))
        {
            // По умолчанию блокируем на 24 часа
            return DateTime.UtcNow.AddDays(1);
        }

        return duration.ToLower() switch
        {
            "1h" => DateTime.UtcNow.AddHours(1),
            "1d" => DateTime.UtcNow.AddDays(1),
            "7d" => DateTime.UtcNow.AddDays(7),
            "30d" => DateTime.UtcNow.AddDays(30),
            "permanent" => null, // null означает постоянную блокировку
            _ => DateTime.UtcNow.AddDays(1) // По умолчанию 1 день
        };
    }
}
