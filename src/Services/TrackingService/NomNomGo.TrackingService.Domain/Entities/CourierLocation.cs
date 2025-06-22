using NomNomGo.TrackingService.Domain.Common;

namespace NomNomGo.TrackingService.Domain.Entities;

public class CourierLocation : BaseEntity {
    public Guid CourierId { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}