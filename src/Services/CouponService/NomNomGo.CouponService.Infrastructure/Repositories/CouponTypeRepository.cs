using Microsoft.EntityFrameworkCore;
using NomNomGo.CouponService.Domain.Entities;
using NomNomGo.CouponService.Domain.Interfaces.Repositories;
using NomNomGo.CouponService.Infrastructure.Persistence.Database;
using NomNomGo.CouponService.Infrastructure.Repositories.Base;

namespace NomNomGo.CouponService.Infrastructure.Repositories;

public class CouponTypeRepository : BaseRepository<CouponType>, ICouponTypeRepository
{
    public CouponTypeRepository(ApplicationDbContext dbContext) : base(dbContext) { }

    public async Task<CouponType?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower(), cancellationToken);
    }
}