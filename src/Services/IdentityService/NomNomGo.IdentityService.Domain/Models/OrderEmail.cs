namespace NomNomGo.IdentityService.Domain.Models;

public class OrderEmail
{
    public string ToEmail { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public string RestaurantName { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = string.Empty;
}