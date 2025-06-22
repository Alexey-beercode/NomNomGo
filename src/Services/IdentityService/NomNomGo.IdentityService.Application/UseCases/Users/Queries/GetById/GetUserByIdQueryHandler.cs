using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NomNomGo.IdentityService.Application.DTOs.Response.Users;
using NomNomGo.IdentityService.Application.Interfaces.CQRS;
using NomNomGo.IdentityService.Application.Models;
using NomNomGo.IdentityService.Domain.Interfaces.UnitOfWork;

namespace NomNomGo.IdentityService.Application.UseCases.Users.Queries.GetById
{
    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDetailResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetUserByIdQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<UserDetailResponse>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _unitOfWork.UserRepository.GetByIdAsync(request.UserId, cancellationToken);
                if (user == null)
                {
                    return Result<UserDetailResponse>.Failure("Пользователь не найден.");
                }

                // Получение ролей пользователя
                var userWithRoles = await _unitOfWork.UserRepository.GetWithRolesAsync(request.UserId, cancellationToken);
                var roleNames = userWithRoles?.UserRoles
                    .Select(ur => ur.Role.Name)
                    .ToList() ?? new List<string>();

                // Получение разрешений пользователя
                var permissions = new List<string>();
                if (userWithRoles?.UserRoles != null)
                {
                    foreach (var userRole in userWithRoles.UserRoles)
                    {
                        var role = await _unitOfWork.RoleRepository.GetWithPermissionsAsync(userRole.RoleId, cancellationToken);
                        if (role?.RolePermissions != null)
                        {
                            var rolePermissions = role.RolePermissions
                                .Select(rp => rp.Permission.Name);
                            
                            permissions.AddRange(rolePermissions);
                        }
                    }
                }

                // Формирование ответа
                var response = new UserDetailResponse
                {
                    UserId = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    IsBlocked = user.IsBlocked,
                    BlockedUntil = user.BlockedUntil,
                    Roles = roleNames,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt
                };

                return Result<UserDetailResponse>.Success(response);
            }
            catch (Exception ex)
            {
                return Result<UserDetailResponse>.Failure($"Ошибка при получении пользователя: {ex.Message}");
            }
        }
    }
}