using NomNomGo.CouponService.Domain.Common;

namespace NomNomGo.CouponService.Domain.Entities;

public class CouponUsage : BaseEntity {
    public Guid CouponId { get; set; }
    public Coupon Coupon { get; set; } = null!;
    public Guid UserId { get; set; }
    public Guid OrderId { get; set; } // Связка с заказом
    public DateTime UsedAt { get; set; } = DateTime.UtcNow;
}