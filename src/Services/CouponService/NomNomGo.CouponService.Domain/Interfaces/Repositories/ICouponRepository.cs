using NomNomGo.CouponService.Domain.Entities;
using NomNomGo.CouponService.Domain.Interfaces.Repositories.Base;

namespace NomNomGo.CouponService.Domain.Interfaces.Repositories;

public interface ICouponRepository : IRepository<Coupon>
{
    Task<Coupon?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Coupon>> GetActiveCouponsAsync(CancellationToken cancellationToken = default);
}