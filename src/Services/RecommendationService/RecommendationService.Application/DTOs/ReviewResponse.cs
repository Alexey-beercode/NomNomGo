namespace RecommendationService.Application.DTOs;

public class ReviewResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid TargetId { get; set; }
    public string TargetType { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public string? Sentiment { get; set; }
    public DateTime CreatedAt { get; set; }
}