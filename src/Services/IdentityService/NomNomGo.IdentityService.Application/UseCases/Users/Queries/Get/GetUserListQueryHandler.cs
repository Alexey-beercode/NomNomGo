using System.Linq.Expressions;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NomNomGo.IdentityService.Application.DTOs.Response.Users;
using NomNomGo.IdentityService.Application.Models;
using NomNomGo.IdentityService.Domain.Entities;
using NomNomGo.IdentityService.Domain.Interfaces.UnitOfWork;

namespace NomNomGo.IdentityService.Application.UseCases.Users.Queries.Get
{
    public class GetUserListQueryHandler : IRequestHandler<GetUserListQuery, Result<PaginatedList<UserListItem>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetUserListQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<PaginatedList<UserListItem>>> Handle(GetUserListQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Создание базового запроса
                var query = _unitOfWork.UserRepository.GetQueryable();

                // Применение фильтров
                query = ApplyFilters(query, request);

                // Подсчет общего количества элементов
                var totalCount = await query.CountAsync(cancellationToken);

                // Применение сортировки
                query = ApplySorting(query, request.SortBy, request.SortDescending);

                // Применение пагинации и загрузка связанных данных
                var users = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                // Преобразование в DTO
                var userListItems = await MapToUserListItemsAsync(users, cancellationToken);

                // Создание пагинированного списка
                var result = new PaginatedList<UserListItem>(
                    userListItems, 
                    totalCount, 
                    request.PageNumber, 
                    request.PageSize);

                return Result<PaginatedList<UserListItem>>.Success(result);
            }
            catch (Exception ex)
            {
                return Result<PaginatedList<UserListItem>>.Failure($"Ошибка при получении списка пользователей: {ex.Message}");
            }
        }

        private static IQueryable<User> ApplyFilters(IQueryable<User> query, GetUserListQuery request)
        {
            // Фильтрация по поисковому термину
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(u => 
                    u.Username.ToLower().Contains(searchTerm) || 
                    u.Email.ToLower().Contains(searchTerm) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(searchTerm)));
            }

            // Фильтрация по статусу блокировки
            if (request.IsBlocked.HasValue)
            {
                query = query.Where(u => u.IsBlocked == request.IsBlocked.Value);
            }

            // Фильтрация по роли
            if (!string.IsNullOrEmpty(request.Role))
            {
                query = query.Where(u => u.UserRoles.Any(ur => ur.Role.Name.ToLower() == request.Role.ToLower()));
            }

            return query;
        }

        private static IQueryable<User> ApplySorting(IQueryable<User> query, string? sortBy, bool sortDescending)
        {
            Expression<Func<User, object>> keySelector = sortBy?.ToLower() switch
            {
                "username" => u => u.Username,
                "email" => u => u.Email,
                "createdat" => u => u.CreatedAt,
                "updatedat" => u => u.UpdatedAt,
                "isblocked" => u => u.IsBlocked,
                _ => u => u.Username
            };

            return sortDescending
                ? query.OrderByDescending(keySelector)
                : query.OrderBy(keySelector);
        }

        private async Task<List<UserListItem>> MapToUserListItemsAsync(List<User> users, CancellationToken cancellationToken)
        {
            var userListItems = new List<UserListItem>();

            foreach (var user in users)
            {
                var roleNames = user.UserRoles.Select(ur => ur.Role.Name).ToList();
                var userListItem = new UserListItem
                {
                    UserId = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    IsBlocked = user.IsBlocked,
                    Roles = roleNames,
                    CreatedAt = user.CreatedAt,
                };
                
                userListItems.Add(userListItem);
            }

            return userListItems;
        }
        
    }
}