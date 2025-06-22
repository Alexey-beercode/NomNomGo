using AutoMapper;
using MediatR;
using NomNomGo.IdentityService.Application.DTOs.Response.Users;
using NomNomGo.IdentityService.Application.Models;
using NomNomGo.IdentityService.Domain.Interfaces.UnitOfWork;

namespace NomNomGo.IdentityService.Application.UseCases.Users.Commands.UnblockUser;

public class UnblockUserCommandHandler : IRequestHandler<UnblockUserCommand, Result<UserDetailResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UnblockUserCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<UserDetailResponse>> Handle(UnblockUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _unitOfWork.UserRepository.GetWithRolesAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                return Result<UserDetailResponse>.Failure("Пользователь не найден.");
            }

            // Проверяем, заблокирован ли пользователь
            if (!user.IsBlocked)
            {
                return Result<UserDetailResponse>.Failure("Пользователь не заблокирован.");
            }

            // Разблокируем пользователя
            user.IsBlocked = false;
            user.BlockedUntil = null;
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
            return Result<UserDetailResponse>.Failure($"Произошла ошибка при разблокировке пользователя: {ex.Message}");
        }
    }
}