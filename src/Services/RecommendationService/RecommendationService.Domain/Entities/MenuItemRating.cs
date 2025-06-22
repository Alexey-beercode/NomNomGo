using System.ComponentModel.DataAnnotations;
using RecommendationService.Domain.Common;

namespace RecommendationService.Domain.Entities;
public class MenuItemRating : BaseEntity
{
    [Required]
    public Guid MenuItemId { get; set; }
        
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}