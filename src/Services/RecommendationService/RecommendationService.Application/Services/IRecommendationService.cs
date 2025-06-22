using RecommendationService.Application.DTOs;

namespace RecommendationService.Application.Services;

public interface IRecommendationService
{
    Task<IEnumerable<RecommendationResponse>> GetPersonalizedRecommendationsAsync(Guid userId, int count = 10);
    Task<IEnumerable<RecommendationResponse>> GetPopularRecommendationsAsync(int count = 10);
    Task AddUserOrderAsync(Guid userId, Guid restaurantId, Guid menuItemId);
    Task UpdatePopularItemsAsync();
}