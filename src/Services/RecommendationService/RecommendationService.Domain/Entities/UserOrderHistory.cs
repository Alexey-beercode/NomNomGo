using System.ComponentModel.DataAnnotations;
using RecommendationService.Domain.Common;

namespace RecommendationService.Domain.Entities;

public class UserOrderHistory : BaseEntity
{
    [Required]
    public Guid UserId { get; set; }
        
    [Required]
    public Guid RestaurantId { get; set; }
        
    [Required]
    public Guid MenuItemId { get; set; }
        
    public DateTime OrderDate { get; set; }
}