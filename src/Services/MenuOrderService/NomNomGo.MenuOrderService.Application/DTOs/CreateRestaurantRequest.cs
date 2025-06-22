namespace NomNomGo.MenuOrderService.Application.DTOs;

public class CreateRestaurantRequest
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}