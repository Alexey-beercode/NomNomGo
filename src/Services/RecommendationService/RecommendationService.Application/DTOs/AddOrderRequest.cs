namespace RecommendationService.Application.DTOs;

public class AddOrderRequest
{
    public Guid UserId { get; set; }
    public Guid RestaurantId { get; set; }
    public Guid MenuItemId { get; set; }
}