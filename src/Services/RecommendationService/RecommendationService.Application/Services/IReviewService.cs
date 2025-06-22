using RecommendationService.Application.DTOs;

namespace RecommendationService.Application.Services;

public interface IReviewService
{
    Task<ReviewResponse> CreateReviewAsync(CreateReviewRequest request);
    Task<IEnumerable<ReviewResponse>> GetReviewsAsync(Guid targetId, string targetType);
    Task<RatingResponse> GetRatingAsync(Guid targetId, string targetType);
    Task UpdateRatingsAsync();
}