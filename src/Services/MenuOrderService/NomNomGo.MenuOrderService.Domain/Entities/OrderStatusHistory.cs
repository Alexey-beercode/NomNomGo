using System.ComponentModel.DataAnnotations;

namespace NomNomGo.MenuOrderService.Domain.Entities;

public class OrderStatusHistory : BaseEntity
{
    [Required]
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
        
    [Required]
    public Guid StatusId { get; set; }
    public OrderStatus Status { get; set; } = null!;
        
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
        
    [MaxLength(500)]
    public string? Comment { get; set; }
}