using System.ComponentModel.DataAnnotations;
using RecommendationService.Domain.Common;

namespace RecommendationService.Domain.Entities;

public class MenuItemReview : BaseEntity
{
    [Required]
    public Guid UserId { get; set; }
        
    [Required]
    public Guid MenuItemId { get; set; }
        
    [Range(1, 5)]
    public int Rating { get; set; }
        
    [MaxLength(1000)]
    public string? Comment { get; set; }
        
    public Guid? SentimentId { get; set; }
    public ReviewSentiment? Sentiment { get; set; }
}