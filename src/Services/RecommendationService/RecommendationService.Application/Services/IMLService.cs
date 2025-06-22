using RecommendationService.ML.Models;

namespace RecommendationService.Application.Services;

public interface IMlService
{
    Task<IEnumerable<ItemRecommendation>> GetRecommendationsAsync(Guid userId, int count = 10);
    Task<SentimentPrediction> AnalyzeSentimentAsync(string text);
    Task TrainModelsAsync();
}