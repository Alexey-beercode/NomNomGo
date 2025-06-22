using System.ComponentModel.DataAnnotations;

namespace NomNomGo.MenuOrderService.Domain.Entities;

public class MenuItemChange : BaseEntity
{
    [Required]
    public Guid MenuItemId { get; set; }
    public MenuItem MenuItem { get; set; } = null!;
        
    public decimal OldPrice { get; set; }
    public decimal NewPrice { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}