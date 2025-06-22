namespace NomNomGo.MenuOrderService.Application.DTOs;

public class RestaurantResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public double AverageRating { get; set; } // Из RecommendationService
    public int ReviewCount { get; set; }
    public string? ImageUrl { get; set; }
}