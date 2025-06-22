using System.ComponentModel.DataAnnotations;

namespace NomNomGo.MenuOrderService.Domain.Entities;

public class MenuItem : BaseEntity
{
    [Required]
    public Guid RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; } = null!;
        
    [Required]
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;
        
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;
        
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
        
    [Required]
    public decimal Price { get; set; }
        
    public bool IsAvailable { get; set; } = true;
        
    [MaxLength(500)]
    public string? ImageUrl { get; set; }
        
    // Navigation
    public List<OrderItem> OrderItems { get; set; } = new();
    public List<MenuItemChange> PriceChanges { get; set; } = new();
}