using System.ComponentModel.DataAnnotations;
using RecommendationService.Domain.Common;

namespace RecommendationService.Domain.Entities;

public class CourierRating : BaseEntity
{
    [Required]
    public Guid CourierId { get; set; }
        
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}