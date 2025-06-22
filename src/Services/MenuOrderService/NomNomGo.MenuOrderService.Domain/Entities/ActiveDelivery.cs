using System.ComponentModel.DataAnnotations;

namespace NomNomGo.MenuOrderService.Domain.Entities;

public class ActiveDelivery : BaseEntity
{
    [Required]
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
        
    [Required]
    public Guid CourierId { get; set; }
        
    public DateTime? EstimatedDeliveryTime { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        
    // Текущие координаты курьера (кэш из Redis)
    public double? CurrentLatitude { get; set; }
    public double? CurrentLongitude { get; set; }
}