using System.ComponentModel.DataAnnotations;
using RecommendationService.Domain.Common;

namespace RecommendationService.Domain.Entities;

public class PopularMenuItem : BaseEntity
{
    [Required]
    public Guid MenuItemId { get; set; }
        
    public int OrderCount { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}