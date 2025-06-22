using NomNomGo.TrackingService.Domain.Common;

namespace NomNomGo.TrackingService.Domain.Entities;

public class ActiveDelivery : BaseEntity {
    public Guid OrderId { get; set; }
    public Guid CourierId { get; set; }
    public DateTime EstimatedDeliveryTime { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}