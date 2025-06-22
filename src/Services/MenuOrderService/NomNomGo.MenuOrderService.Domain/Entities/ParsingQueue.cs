using System.ComponentModel.DataAnnotations;

namespace NomNomGo.MenuOrderService.Domain.Entities;

public class ParsingQueue : BaseEntity
{
    [Required]
    public Guid RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; } = null!;
        
    [Required]
    public Guid StatusId { get; set; }
    public ParsingStatus Status { get; set; } = null!;
        
    [MaxLength(500)]
    public string? SourceUrl { get; set; }
        
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
        
    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }
}