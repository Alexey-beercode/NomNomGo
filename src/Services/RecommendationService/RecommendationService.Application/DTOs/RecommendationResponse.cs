namespace RecommendationService.Application.DTOs;

public class RecommendationResponse
{
    public Guid MenuItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public Guid RestaurantId { get; set; }  
    public string RestaurantName { get; set; } = string.Empty;
    public double Score { get; set; } // Вероятность понравится
    public string Reason { get; set; } = string.Empty; // "Popular", "Similar taste", etc.
}