namespace NomNomGo.MenuOrderService.Application.DTOs;

public class CreateOrderRequest
{
    public Guid UserId { get; set; }
    public Guid RestaurantId { get; set; }
    public string DeliveryAddress { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public List<OrderItemRequest> Items { get; set; } = new();
}