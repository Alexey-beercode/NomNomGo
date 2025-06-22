using NomNomGo.CouponService.Domain.Entities;
using NomNomGo.CouponService.Domain.Interfaces.Repositories.Base;

namespace NomNomGo.CouponService.Domain.Interfaces.Repositories;

public interface ICouponUsageRepository : IRepository<CouponUsage>
{
    Task<IReadOnlyList<CouponUsage>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<int> CountCouponUsageAsync(Guid couponId, Guid userId, CancellationToken cancellationToken = default);
}