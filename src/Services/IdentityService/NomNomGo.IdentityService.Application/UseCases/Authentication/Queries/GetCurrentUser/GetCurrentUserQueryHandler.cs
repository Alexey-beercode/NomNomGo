using AutoMapper;
using MediatR;
using NomNomGo.IdentityService.Application.DTOs.Response.Authentication;
using NomNomGo.IdentityService.Application.Interfaces.Services;
using NomNomGo.IdentityService.Application.Models;
using NomNomGo.IdentityService.Domain.Entities.Relationships;
using NomNomGo.IdentityService.Domain.Exceptions;
using NomNomGo.IdentityService.Domain.Interfaces.UnitOfWork;

namespace NomNomGo.IdentityService.Application.UseCases.Authentication.Queries.GetCurrentUser
{
    public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, Result<CurrentUserResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public GetCurrentUserQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async Task<Result<CurrentUserResponse>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (userId == Guid.Empty)
            {
                throw new AuthenticationException("Пользователь не аутентифицирован.");
            }

            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                throw new NotFoundException("Пользователь не найден.");
            }

            // Получение ролей пользователя
            var userWithRoles = await _unitOfWork.UserRepository.GetWithRolesAsync(userId, cancellationToken);
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
            var response= _mapper.Map<CurrentUserResponse>(user);
            
            response.Permissions = permissions.Distinct();
            response.Roles = roleNames;

            return Result<CurrentUserResponse>.Success(response);
        }
    }
}