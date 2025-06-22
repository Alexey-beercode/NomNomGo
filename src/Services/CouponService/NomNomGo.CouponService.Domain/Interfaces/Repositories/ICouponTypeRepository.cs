using NomNomGo.CouponService.Domain.Entities;
using NomNomGo.CouponService.Domain.Interfaces.Repositories.Base;

namespace NomNomGo.CouponService.Domain.Interfaces.Repositories;

public interface ICouponTypeRepository : IRepository<CouponType>
{
    Task<CouponType?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}