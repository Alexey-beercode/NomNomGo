using Microsoft.EntityFrameworkCore;
using NomNomGo.CouponService.Domain.Entities;
using NomNomGo.CouponService.Domain.Interfaces.Repositories;
using NomNomGo.CouponService.Infrastructure.Persistence.Database;
using NomNomGo.CouponService.Infrastructure.Repositories.Base;

namespace NomNomGo.CouponService.Infrastructure.Repositories;

public class CouponRepository : BaseRepository<Coupon>, ICouponRepository
{
    public CouponRepository(ApplicationDbContext dbContext) : base(dbContext) { }

    public async Task<Coupon?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.CouponType)
            .FirstOrDefaultAsync(c => c.Code.ToLower() == code.ToLower(), cancellationToken);
    }

    public async Task<IReadOnlyList<Coupon>> GetActiveCouponsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(c => c.IsActive && (c.ExpiryDate == null || c.ExpiryDate > DateTime.UtcNow))
            .ToListAsync(cancellationToken);
    }
}