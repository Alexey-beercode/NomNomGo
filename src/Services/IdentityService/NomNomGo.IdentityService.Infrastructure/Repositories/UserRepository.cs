using Microsoft.EntityFrameworkCore;
using NomNomGo.IdentityService.Domain.Entities;
using NomNomGo.IdentityService.Domain.Interfaces.Repositories;
using NomNomGo.IdentityService.Infrastructure.Persistence.Database;
using NomNomGo.IdentityService.Infrastructure.Repositories.Base;

namespace NomNomGo.IdentityService.Infrastructure.Repositories
{
    /// <summary>
    /// Реализация репозитория пользователей
    /// </summary>
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext dbContext) : base(dbContext) { }

        /// <inheritdoc />
        public async Task<User> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<User> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower(), cancellationToken);
        }

        /// <inheritdoc />
        public async Task<User> GetWithRolesAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        }
        
        /// <inheritdoc />
public async Task<(IEnumerable<User> Users, int TotalCount)> GetUsersWithFiltersAsync(
    string? search = null,
    string? role = null,
    bool? isBlocked = null,
    int page = 1,
    int pageSize = 10,
    CancellationToken cancellationToken = default)
{
    var query = _dbSet
        .Include(u => u.UserRoles)
        .ThenInclude(ur => ur.Role)
        .AsQueryable();

    // Поиск по имени пользователя или email
    if (!string.IsNullOrEmpty(search))
    {
        var searchLower = search.ToLower();
        query = query.Where(u => 
            u.Username.ToLower().Contains(searchLower) ||
            u.Email.ToLower().Contains(searchLower));
    }

    // Фильтр по роли
    if (!string.IsNullOrEmpty(role))
    {
        query = query.Where(u => u.UserRoles.Any(ur => ur.Role.Name == role));
    }

    // Фильтр по статусу блокировки
    if (isBlocked.HasValue)
    {
        if (isBlocked.Value)
        {
            // Заблокированные пользователи
            query = query.Where(u => u.IsBlocked && 
                (u.BlockedUntil == null || u.BlockedUntil > DateTime.UtcNow));
        }
        else
        {
            // Активные пользователи
            query = query.Where(u => !u.IsBlocked || 
                (u.BlockedUntil.HasValue && u.BlockedUntil <= DateTime.UtcNow));
        }
    }

    var totalCount = await query.CountAsync(cancellationToken);

    var users = await query
        .OrderByDescending(u => u.CreatedAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .AsNoTracking()
        .ToListAsync(cancellationToken);

    return (users, totalCount);
}

/// <inheritdoc />
public async Task<IEnumerable<User>> GetBlockedUsersAsync(CancellationToken cancellationToken = default)
{
    return await _dbSet
        .Include(u => u.UserRoles)
        .ThenInclude(ur => ur.Role)
        .Where(u => u.IsBlocked && 
            (u.BlockedUntil == null || u.BlockedUntil > DateTime.UtcNow))
        .AsNoTracking()
        .ToListAsync(cancellationToken);
}

/// <inheritdoc />
public async Task<bool> IsUserBlockedAsync(Guid userId, CancellationToken cancellationToken = default)
{
    var user = await _dbSet
        .AsNoTracking()
        .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

    if (user == null || !user.IsBlocked)
        return false;

    // Если есть дата окончания блокировки, проверяем её
    if (user.BlockedUntil.HasValue)
        return user.BlockedUntil > DateTime.UtcNow;

    // Если даты нет, значит блокировка постоянная
    return true;
}
    }
}