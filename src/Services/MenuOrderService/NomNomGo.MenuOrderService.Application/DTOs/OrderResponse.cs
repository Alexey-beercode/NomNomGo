namespace NomNomGo.MenuOrderService.Application.DTOs;

public class OrderResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? CourierId { get; set; }
    public RestaurantResponse Restaurant { get; set; } = null!;
    public decimal TotalPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? EstimatedDeliveryTime { get; set; }
    public string DeliveryAddress { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public List<OrderItemResponse> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}