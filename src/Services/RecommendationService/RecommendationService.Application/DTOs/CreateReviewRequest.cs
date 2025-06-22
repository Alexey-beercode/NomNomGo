namespace RecommendationService.Application.DTOs;

public class CreateReviewRequest
{
    public Guid UserId { get; set; }
    public Guid TargetId { get; set; } // RestaurantId, MenuItemId, или CourierId
    public string TargetType { get; set; } = string.Empty; // "Restaurant", "MenuItem", "Courier"
    public int Rating { get; set; }
    public string? Comment { get; set; }
}