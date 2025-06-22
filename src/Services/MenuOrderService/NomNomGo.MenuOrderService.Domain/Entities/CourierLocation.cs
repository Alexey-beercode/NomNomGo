using System.ComponentModel.DataAnnotations;

namespace NomNomGo.MenuOrderService.Domain.Entities;

public class CourierLocation : BaseEntity
{
    [Required]
    public Guid CourierId { get; set; }
        
    [Required]
    public double Latitude { get; set; }
        
    [Required]
    public double Longitude { get; set; }
        
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}