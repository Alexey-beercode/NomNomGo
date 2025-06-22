namespace NomNomGo.IdentityService.Application.DTOs.Request;

public class OrderEmailRequest
{
    public string Email { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public string RestaurantName { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = string.Empty;
}