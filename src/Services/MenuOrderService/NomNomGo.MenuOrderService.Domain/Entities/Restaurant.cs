using System.ComponentModel.DataAnnotations;

namespace NomNomGo.MenuOrderService.Domain.Entities;

public class Restaurant : BaseEntity
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
        
    [Required, MaxLength(500)]
    public string Address { get; set; } = string.Empty;
        
    [MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;
        
    public bool IsActive { get; set; } = true;
        
    // Navigation
    public List<MenuItem> MenuItems { get; set; } = new();
    public List<Order> Orders { get; set; } = new();
}