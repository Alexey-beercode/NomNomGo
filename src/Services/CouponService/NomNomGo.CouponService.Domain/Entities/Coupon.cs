using NomNomGo.CouponService.Domain.Common;

namespace NomNomGo.CouponService.Domain.Entities;

public class Coupon : BaseEntity {
    public string Code { get; set; } = string.Empty;
    public decimal Discount { get; set; }
    public Guid CouponTypeId { get; set; }
    public CouponType CouponType { get; set; } = null!;
    public int? MaxUses { get; set; } // Если NULL, то без ограничений
    public int Uses { get; set; } = 0;
    public int? UserLimit { get; set; } // Максимальное число использований одним пользователем
    public DateTime? ExpiryDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}