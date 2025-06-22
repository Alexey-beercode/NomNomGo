using Microsoft.EntityFrameworkCore;
using NomNomGo.CouponService.Domain.Entities;
using NomNomGo.CouponService.Domain.Interfaces.Repositories;
using NomNomGo.CouponService.Infrastructure.Persistence.Database;
using NomNomGo.CouponService.Infrastructure.Repositories.Base;

namespace NomNomGo.CouponService.Infrastructure.Repositories;

public class CouponUsageRepository : BaseRepository<CouponUsage>, ICouponUsageRepository
{
    public CouponUsageRepository(ApplicationDbContext dbContext) : base(dbContext) { }

    public async Task<IReadOnlyList<CouponUsage>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(u => u.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountCouponUsageAsync(Guid couponId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .CountAsync(u => u.CouponId == couponId && u.UserId == userId, cancellationToken);
    }
}