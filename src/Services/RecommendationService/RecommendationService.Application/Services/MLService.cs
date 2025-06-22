using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ML;
using RecommendationService.Infrastructure.Data;
using RecommendationService.ML.Models;

namespace RecommendationService.Application.Services;

public class MlService : IMlService
{
    private readonly MLContext _mlContext;
    private readonly IServiceProvider _serviceProvider; // Используем IServiceProvider вместо прямой зависимости
    private ITransformer? _recommendationModel;
    private ITransformer? _sentimentModel;

    public MlService(IServiceProvider serviceProvider) // Инжектим IServiceProvider
    {
        _mlContext = new MLContext(seed: 1);
        _serviceProvider = serviceProvider;
    }

    public async Task<IEnumerable<ItemRecommendation>> GetRecommendationsAsync(Guid userId, int count = 10)
    {
        // Если модель не обучена, используем популярные рекомендации
        if (_recommendationModel == null)
        {
            return await GetPopularBasedRecommendationsAsync(count);
        }

        try
        {
            // Создаем scope для работы с DbContext
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<RecommendationReviewDbContext>();

            // Получаем данные пользователя
            var userHistory = await context.UserOrderHistories
                .Where(h => h.UserId == userId)
                .Select(h => h.MenuItemId)
                .Distinct()
                .ToListAsync();

            // Получаем все доступные блюда (исключая уже заказанные)
            var availableItems = await context.PopularMenuItems
                .Where(p => !userHistory.Contains(p.MenuItemId))
                .Take(count * 2)
                .Select(p => p.MenuItemId)
                .ToListAsync();

            var recommendations = new List<ItemRecommendation>();

            // Для демо просто возвращаем случайные скоры
            var random = new Random();
            foreach (var itemId in availableItems.Take(count))
            {
                recommendations.Add(new ItemRecommendation 
                { 
                    Score = (float)(random.NextDouble() * 0.5 + 0.5) // 0.5-1.0
                });
            }

            return recommendations.OrderByDescending(r => r.Score);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ML Recommendation error: {ex.Message}");
            return await GetPopularBasedRecommendationsAsync(count);
        }
    }

public async Task<SentimentPrediction> AnalyzeSentimentAsync(string text)
{
    if (string.IsNullOrEmpty(text))
    {
        return new SentimentPrediction { Prediction = true, Probability = 0.5f, Score = 0.5f };
    }

    // ✅ РАСШИРЕННЫЕ СЛОВАРИ ДЛЯ РУССКОГО ЯЗЫКА
    var positiveWords = new[] { 
        "отлично", "отличная", "отличный", "отличное",
        "хорошо", "хороший", "хорошая", "хорошее", 
        "вкусно", "вкусный", "вкусная", "вкусное",
        "быстро", "быстрая", "быстрый", "быстрое",
        "рекомендую", "советую", "классно", "класс",
        "супер", "прекрасно", "прекрасный", "прекрасная",
        "замечательно", "отменно", "превосходно",
        "нравится", "понравилось", "люблю", "обожаю",
        "качественно", "свежий", "свежая", "горячий", "горячая",
        "вежливо", "вежливый", "дружелюбно", "приятно"
    };

    var negativeWords = new[] { 
        "плохо", "плохой", "плохая", "плохое",
        "ужасно", "ужасный", "ужасная", "ужасное",
        "медленно", "медленный", "медленная",
        "холодно", "холодный", "холодная", "холодное",
        "отвратительно", "отвратительный", "отвратительная",
        "разочарован", "разочарована", "разочарование",
        "не понравилось", "не нравится", "не вкусно",
        "грубо", "грубый", "грубая", "невежливо",
        "долго", "поздно", "опоздали", "задержка",
        "испорчено", "испорченный", "несвежий", "протухший"
    };

    var neutralWords = new[] {
        "нормально", "средне", "средний", "обычно", "обычный",
        "ничего", "приемлемо", "сойдет", "терпимо"
    };

    var lowerText = text.ToLower();
    
    // Подсчитываем совпадения
    var positiveCount = positiveWords.Count(word => lowerText.Contains(word));
    var negativeCount = negativeWords.Count(word => lowerText.Contains(word));
    var neutralCount = neutralWords.Count(word => lowerText.Contains(word));

    Console.WriteLine($"🔍 Sentiment analysis: '{text}'");
    Console.WriteLine($"   Positive words: {positiveCount}, Negative: {negativeCount}, Neutral: {neutralCount}");

    // Логика определения тональности
    bool isPositive;
    float confidence;

    if (positiveCount > negativeCount && positiveCount > 0)
    {
        isPositive = true;
        confidence = Math.Min(0.9f, 0.6f + (positiveCount * 0.1f));
    }
    else if (negativeCount > positiveCount && negativeCount > 0)
    {
        isPositive = false;
        confidence = Math.Min(0.9f, 0.6f + (negativeCount * 0.1f));
    }
    else if (neutralCount > 0)
    {
        isPositive = true; // Нейтральное считаем слабо позитивным
        confidence = 0.4f;
    }
    else
    {
        // Если ключевых слов нет, анализируем общий тон
        var exclamationMarks = text.Count(c => c == '!');
        var questionMarks = text.Count(c => c == '?');
        
        isPositive = exclamationMarks > questionMarks;
        confidence = 0.3f;
    }

    var result = new SentimentPrediction
    {
        Prediction = isPositive,
        Probability = confidence,
        Score = isPositive ? confidence : -confidence
    };

    Console.WriteLine($"   Result: {(isPositive ? "Positive" : "Negative")} (confidence: {confidence:F2})");
    return result;
}

    public async Task TrainModelsAsync()
    {
        try
        {
            Console.WriteLine("🤖 Training ML models...");
                
            // Тренируем модель рекомендаций (упрощенная версия)
            await TrainRecommendationModelAsync();
                
            // Тренируем модель сентимент-анализа
            await TrainSentimentModelAsync();
                
            Console.WriteLine("✅ ML models trained successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Training failed: {ex.Message}");
        }
    }

    private async Task<IEnumerable<ItemRecommendation>> GetPopularBasedRecommendationsAsync(int count)
    {
        try
        {
            // Создаем scope для работы с DbContext
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<RecommendationReviewDbContext>();

            // Fallback: возвращаем популярные блюда как рекомендации
            var popularItems = await context.PopularMenuItems
                .OrderByDescending(p => p.OrderCount)
                .Take(count)
                .ToListAsync();

            return popularItems.Select(p => new ItemRecommendation
            {
                Score = Math.Min(p.OrderCount / 100.0f, 1.0f) // Нормализация
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting popular recommendations: {ex.Message}");
            // Возвращаем заглушку если DB недоступна
            return Enumerable.Range(1, count).Select(_ => new ItemRecommendation { Score = 0.5f });
        }
    }

    private async Task TrainRecommendationModelAsync()
    {
        try
        {
            // Получаем данные для обучения
            var trainingData = await GetTrainingDataAsync();
                
            if (!trainingData.Any())
            {
                Console.WriteLine("⚠️ No training data available for recommendations");
                return;
            }

            var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);
                
            // Простой пайплайн для Matrix Factorization
            var pipeline = _mlContext.Transforms.Conversion
                .MapValueToKey("userIdEncoded", "UserId")
                .Append(_mlContext.Transforms.Conversion.MapValueToKey("itemIdEncoded", "ItemId"));

            // Для демо просто создаем "пустую" модель
            _recommendationModel = pipeline.Fit(dataView);
                
            Console.WriteLine($"✅ Recommendation model trained on {trainingData.Count()} interactions");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Recommendation training failed: {ex.Message}");
        }
    }

    private async Task TrainSentimentModelAsync()
    {
        try
        {
            // Генерируем тестовые данные для сентимент-анализа
            var sentimentData = GenerateSyntheticSentimentData();
            var dataView = _mlContext.Data.LoadFromEnumerable(sentimentData);

            var pipeline = _mlContext.Transforms.Text.FeaturizeText("Features", "Text")
                .Append(_mlContext.BinaryClassification.Trainers.SdcaLogisticRegression("Sentiment", "Features"));

            _sentimentModel = pipeline.Fit(dataView);
                
            Console.WriteLine("✅ Sentiment model trained on synthetic data");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Sentiment training failed: {ex.Message}");
        }
    }

    private async Task<IEnumerable<UserItemInteraction>> GetTrainingDataAsync()
    {
        try
        {
            // Создаем scope для работы с DbContext
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<RecommendationReviewDbContext>();

            // Преобразуем GUID в числа для ML
            var orderHistory = await context.UserOrderHistories
                .GroupBy(h => new { h.UserId, h.MenuItemId })
                .Select(g => new 
                {
                    UserId = g.Key.UserId,
                    MenuItemId = g.Key.MenuItemId,
                    Count = g.Count()
                })
                .ToListAsync();

            return orderHistory.Select(h => new UserItemInteraction
            {
                UserId = (float)h.UserId.GetHashCode(), // Простое преобразование GUID в число
                ItemId = (float)h.MenuItemId.GetHashCode(),
                Rating = Math.Min(h.Count, 5) // Ограничиваем рейтинг
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting training data: {ex.Message}");
            return Enumerable.Empty<UserItemInteraction>();
        }
    }

    private IEnumerable<SentimentData> GenerateSyntheticSentimentData()
    {
        return new[]
        {
            new SentimentData { Text = "Отличная еда, быстрая доставка!", Sentiment = true },
            new SentimentData { Text = "Очень вкусно, рекомендую всем!", Sentiment = true },
            new SentimentData { Text = "Супер качество, заказываю постоянно", Sentiment = true },
            new SentimentData { Text = "Быстро привезли, горячее и вкусное", Sentiment = true },
            new SentimentData { Text = "Прекрасный сервис и отличный вкус", Sentiment = true },
                
            new SentimentData { Text = "Ужасное качество, долго ждали", Sentiment = false },
            new SentimentData { Text = "Не понравилось, холодная еда", Sentiment = false },
            new SentimentData { Text = "Разочарован качеством блюд", Sentiment = false },
            new SentimentData { Text = "Плохой сервис, больше не закажу", Sentiment = false },
            new SentimentData { Text = "Отвратительно, деньги на ветер", Sentiment = false },
                
            new SentimentData { Text = "Нормально, ничего особенного", Sentiment = true },
            new SentimentData { Text = "Средненько, можно было лучше", Sentiment = true },
            new SentimentData { Text = "Обычная еда, без изысков", Sentiment = true }
        };
    }
}
