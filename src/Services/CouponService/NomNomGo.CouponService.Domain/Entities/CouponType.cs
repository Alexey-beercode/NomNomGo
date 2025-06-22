using NomNomGo.CouponService.Domain.Common;

namespace NomNomGo.CouponService.Domain.Entities;

public class CouponType : BaseEntity {
    public string Name { get; set; } = string.Empty; // (Unique, General)
    public ICollection<Coupon> Coupons { get; set; } = new List<Coupon>();
}