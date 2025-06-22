using System.ComponentModel.DataAnnotations;

namespace NomNomGo.MenuOrderService.Domain.Entities;

public class OrderItem : BaseEntity
{
    [Required]
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
        
    [Required]
    public Guid MenuItemId { get; set; }
    public MenuItem MenuItem { get; set; } = null!;
        
    [Required]
    public int Quantity { get; set; }
        
    [Required]
    public decimal Price { get; set; } // Цена на момент заказа
}