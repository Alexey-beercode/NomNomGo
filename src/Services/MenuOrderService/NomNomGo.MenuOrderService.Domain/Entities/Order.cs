using System.ComponentModel.DataAnnotations;

namespace NomNomGo.MenuOrderService.Domain.Entities;

public class Order : BaseEntity
{
    [Required]
    public Guid UserId { get; set; }
        
    public Guid? CourierId { get; set; }
        
    [Required]
    public Guid RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; } = null!;
        
    [Required]
    public decimal TotalPrice { get; set; }
        
    public decimal DiscountAmount { get; set; } = 0;
        
    [Required]
    public Guid StatusId { get; set; }
    public OrderStatus Status { get; set; } = null!;
        
    public DateTime? EstimatedDeliveryTime { get; set; }
        
    [MaxLength(500)]
    public string DeliveryAddress { get; set; } = string.Empty;
        
    [MaxLength(1000)]
    public string? Notes { get; set; }
        
    // Navigation
    public List<OrderItem> Items { get; set; } = new();
    public List<OrderStatusHistory> StatusHistory { get; set; } = new();
    public ActiveDelivery? ActiveDelivery { get; set; }
}