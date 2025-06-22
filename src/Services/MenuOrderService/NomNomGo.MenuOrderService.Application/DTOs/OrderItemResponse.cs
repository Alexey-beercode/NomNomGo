namespace NomNomGo.MenuOrderService.Application.DTOs;

public class OrderItemResponse
{
    public Guid Id { get; set; }
    public MenuItemResponse MenuItem { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}