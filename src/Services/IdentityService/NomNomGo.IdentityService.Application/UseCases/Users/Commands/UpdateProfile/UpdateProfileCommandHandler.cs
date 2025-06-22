using AutoMapper;
using MediatR;
using NomNomGo.IdentityService.Application.DTOs.Response.Users;
using NomNomGo.IdentityService.Application.Interfaces.CQRS;
using NomNomGo.IdentityService.Application.Models;
using NomNomGo.IdentityService.Application.Interfaces.Services;
using NomNomGo.IdentityService.Domain.Interfaces.UnitOfWork;

namespace NomNomGo.IdentityService.Application.UseCases.Users.Commands.UpdateProfile
{
    public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, Result<UpdateProfileResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public UpdateProfileCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async Task<Result<UpdateProfileResponse>> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (userId == Guid.Empty)
            {
                return Result<UpdateProfileResponse>.Failure("Пользователь не аутентифицирован.");
            }

            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return Result<UpdateProfileResponse>.Failure("Пользователь не найден.");
            }

            // Проверка блокировки пользователя
            if (user.IsBlocked)
            {
                var blockMessage = user.BlockedUntil.HasValue
                    ? $"Ваша учетная запись заблокирована до {user.BlockedUntil.Value:g}."
                    : "Ваша учетная запись заблокирована.";
                
                return Result<UpdateProfileResponse>.Failure(blockMessage);
            }

            var validationErrors = new List<string>();

            // Проверка на уникальность email, если он изменен
            if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
            {
                var existingUserByEmail = await _unitOfWork.UserRepository.GetByEmailAsync(request.Email, cancellationToken);
                if (existingUserByEmail != null)
                {
                    validationErrors.Add("Пользователь с таким Email уже существует.");
                }
                else
                {
                    user.Email = request.Email;
                }
            }

            // Проверка на уникальность username, если он изменен
            if (!string.IsNullOrEmpty(request.Username) && request.Username != user.Username)
            {
                var existingUserByUsername = await _unitOfWork.UserRepository.GetByUsernameAsync(request.Username, cancellationToken);
                if (existingUserByUsername != null)
                {
                    validationErrors.Add("Пользователь с таким именем уже существует.");
                }
                else
                {
                    user.Username = request.Username;
                }
            }

            // Если есть ошибки валидации, возвращаем их
            if (validationErrors.Any())
            {
                return Result<UpdateProfileResponse>.Failure(validationErrors.ToString());
            }

            // Обновление телефона, если он изменен
            if (request.PhoneNumber != user.PhoneNumber)
            {
                user.PhoneNumber = request.PhoneNumber;
            }

            // Обновление временной метки
            user.UpdatedAt = DateTime.UtcNow;

            // Сохранение изменений
            _unitOfWork.UserRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Формирование ответа
            var response = new UpdateProfileResponse
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                UpdatedAt = user.UpdatedAt
            };

            return Result<UpdateProfileResponse>.Success(response);
        }
    }
}