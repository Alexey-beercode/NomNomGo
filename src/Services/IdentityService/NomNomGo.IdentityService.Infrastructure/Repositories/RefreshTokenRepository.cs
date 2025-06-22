using Microsoft.EntityFrameworkCore;
using NomNomGo.IdentityService.Domain.Entities;
using NomNomGo.IdentityService.Domain.Interfaces.Repositories;
using NomNomGo.IdentityService.Infrastructure.Persistence.Database;
using NomNomGo.IdentityService.Infrastructure.Repositories.Base;

namespace NomNomGo.IdentityService.Infrastructure.Repositories
{
    /// <summary>
    /// Реализация репозитория токенов обновления
    /// </summary>
    public class RefreshTokenRepository : BaseRepository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(ApplicationDbContext dbContext) : base(dbContext) { }

        /// <inheritdoc />
        public async Task<RefreshToken> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<RefreshToken>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            
            return await _dbSet
                .AsNoTracking()
                .Where(rt => rt.UserId == userId && rt.ExpiresAt > now)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<int> RemoveExpiredTokensAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var expiredTokens = await _dbSet
                .Where(rt => rt.ExpiresAt <= now)
                .ToListAsync(cancellationToken);
                
            _dbSet.RemoveRange(expiredTokens);
            
            return expiredTokens.Count;
        }
    }
}