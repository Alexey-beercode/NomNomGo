using System.ComponentModel.DataAnnotations;
using RecommendationService.Domain.Common;

namespace RecommendationService.Domain.Entities;

public class ReviewSentiment : BaseEntity
{
    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty; // Positive, Negative, Neutral
}