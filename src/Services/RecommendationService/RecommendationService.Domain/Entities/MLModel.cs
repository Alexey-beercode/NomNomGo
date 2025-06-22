using System.ComponentModel.DataAnnotations;
using RecommendationService.Domain.Common;

public class MLModel : BaseEntity
{
    [Required, MaxLength(100)]
    public string ModelName { get; set; } = string.Empty;
        
    [Required, MaxLength(20)]
    public string Version { get; set; } = string.Empty;
}