using System.ComponentModel.DataAnnotations;

namespace NomNomGo.MenuOrderService.Domain.Entities;

public class ParsingStatus : BaseEntity
{
    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty; // Pending, InProgress, Completed, Failed
}