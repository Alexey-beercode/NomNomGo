using Microsoft.EntityFrameworkCore;
using RecommendationService.Application.DTOs;
using RecommendationService.Domain.Entities;
using RecommendationService.Infrastructure.Data;

namespace RecommendationService.Application.Services;

public class ReviewService : IReviewService
{
    private readonly RecommendationReviewDbContext _context;
    private readonly IMlService _mlService;

    public ReviewService(RecommendationReviewDbContext context, IMlService mlService)
    {
        _context = context;
        _mlService = mlService;
    }

   public async Task<ReviewResponse> CreateReviewAsync(CreateReviewRequest request)
{
    // Анализ тональности комментария
    Guid? sentimentId = null;
    string? sentimentName = null; // Добавляем переменную для названия
    
    if (!string.IsNullOrEmpty(request.Comment))
    {
        var sentiment = await _mlService.AnalyzeSentimentAsync(request.Comment);
        sentimentName = sentiment.Prediction ? "Positive" : "Negative"; // Сохраняем название
        
        var sentimentEntity = await _context.ReviewSentiments
            .FirstOrDefaultAsync(s => s.Name == sentimentName);
        
        if (sentimentEntity == null)
        {
            sentimentEntity = new ReviewSentiment { Name = sentimentName };
            _context.ReviewSentiments.Add(sentimentEntity);
            await _context.SaveChangesAsync();
        }
        
        sentimentId = sentimentEntity.Id;
    }

    // Создаем отзыв в зависимости от типа
    Guid reviewId;
    DateTime createdAt = DateTime.UtcNow; // Сохраняем время создания
    
    switch (request.TargetType.ToLower())
    {
        case "restaurant":
            var restaurantReview = new RestaurantReview
            {
                UserId = request.UserId,
                RestaurantId = request.TargetId,
                Rating = request.Rating,
                Comment = request.Comment,
                SentimentId = sentimentId,
                CreatedAt = createdAt // Устанавливаем время
            };
            _context.RestaurantReviews.Add(restaurantReview);
            reviewId = restaurantReview.Id;
            break;

        case "menuitem":
            var menuItemReview = new MenuItemReview
            {
                UserId = request.UserId,
                MenuItemId = request.TargetId,
                Rating = request.Rating,
                Comment = request.Comment,
                SentimentId = sentimentId,
                CreatedAt = createdAt
            };
            _context.MenuItemReviews.Add(menuItemReview);
            reviewId = menuItemReview.Id;
            break;

        case "courier":
            var courierReview = new CourierReview
            {
                UserId = request.UserId,
                CourierId = request.TargetId,
                Rating = request.Rating,
                Comment = request.Comment,
                SentimentId = sentimentId,
                CreatedAt = createdAt
            };
            _context.CourierReviews.Add(courierReview);
            reviewId = courierReview.Id;
            break;

        default:
            throw new ArgumentException("Invalid target type");
    }

    await _context.SaveChangesAsync();

    // Обновляем средний рейтинг
    await UpdateRatingForTargetAsync(request.TargetId, request.TargetType);

    // ✅ ИСПРАВЛЕНИЕ: Возвращаем корректный ответ с sentiment
    var response = new ReviewResponse
    {
        Id = reviewId,
        UserId = request.UserId,
        TargetId = request.TargetId,
        TargetType = request.TargetType,
        Rating = request.Rating,
        Comment = request.Comment,
        Sentiment = sentimentName,
        CreatedAt = createdAt
    };
    
    Console.WriteLine($"✅ Review created successfully: Id={response.Id}, Sentiment={response.Sentiment}");
    return response;
}

    public async Task<IEnumerable<ReviewResponse>> GetReviewsAsync(Guid targetId, string targetType)
    {
        return targetType.ToLower() switch
        {
            "restaurant" => await _context.RestaurantReviews
                .Where(r => r.RestaurantId == targetId)
                .Include(r => r.Sentiment)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewResponse
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    TargetId = r.RestaurantId,
                    TargetType = "Restaurant",
                    Rating = r.Rating,
                    Comment = r.Comment,
                    Sentiment = r.Sentiment != null ? r.Sentiment.Name : null,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync(),

            "menuitem" => await _context.MenuItemReviews
                .Where(r => r.MenuItemId == targetId)
                .Include(r => r.Sentiment)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewResponse
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    TargetId = r.MenuItemId,
                    TargetType = "MenuItem",
                    Rating = r.Rating,
                    Comment = r.Comment,
                    Sentiment = r.Sentiment != null ? r.Sentiment.Name : null,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync(),

            _ => new List<ReviewResponse>()
        };
    }

   public async Task<RatingResponse> GetRatingAsync(Guid targetId, string targetType)
{
    Console.WriteLine($"🔍 Getting rating for {targetType} {targetId}");

    switch (targetType.ToLower())
    {
        case "restaurant":
            // Сначала пытаемся получить существующий рейтинг
            var restaurantRating = await _context.RestaurantRatings
                .Where(r => r.RestaurantId == targetId)
                .FirstOrDefaultAsync();

            // Если рейтинга нет, но есть отзывы - пересчитываем
            if (restaurantRating == null)
            {
                var hasReviews = await _context.RestaurantReviews
                    .AnyAsync(r => r.RestaurantId == targetId);
                
                if (hasReviews)
                {
                    Console.WriteLine("   No rating found but reviews exist, recalculating...");
                    await UpdateRatingForTargetAsync(targetId, "Restaurant");
                    
                    // Получаем свежий рейтинг
                    restaurantRating = await _context.RestaurantRatings
                        .Where(r => r.RestaurantId == targetId)
                        .FirstOrDefaultAsync();
                }
            }

            if (restaurantRating != null)
            {
                Console.WriteLine($"   ✅ Found rating: {restaurantRating.AverageRating:F2} ({restaurantRating.ReviewCount} reviews)");
                return new RatingResponse
                {
                    TargetId = restaurantRating.RestaurantId,
                    TargetType = "Restaurant",
                    AverageRating = restaurantRating.AverageRating,
                    ReviewCount = restaurantRating.ReviewCount,
                    LastUpdated = restaurantRating.LastUpdated
                };
            }
            else
            {
                Console.WriteLine("   ⚠️ No rating or reviews found");
                return new RatingResponse 
                { 
                    TargetId = targetId, 
                    TargetType = "Restaurant",
                    AverageRating = 0,
                    ReviewCount = 0,
                    LastUpdated = DateTime.UtcNow
                };
            }

        case "menuitem":
            var menuItemRating = await _context.MenuItemRatings
                .Where(r => r.MenuItemId == targetId)
                .FirstOrDefaultAsync();

            if (menuItemRating == null)
            {
                var hasReviews = await _context.MenuItemReviews
                    .AnyAsync(r => r.MenuItemId == targetId);
                
                if (hasReviews)
                {
                    await UpdateRatingForTargetAsync(targetId, "MenuItem");
                    menuItemRating = await _context.MenuItemRatings
                        .Where(r => r.MenuItemId == targetId)
                        .FirstOrDefaultAsync();
                }
            }

            if (menuItemRating != null)
            {
                return new RatingResponse
                {
                    TargetId = menuItemRating.MenuItemId,
                    TargetType = "MenuItem",
                    AverageRating = menuItemRating.AverageRating,
                    ReviewCount = menuItemRating.ReviewCount,
                    LastUpdated = menuItemRating.LastUpdated
                };
            }
            else
            {
                return new RatingResponse 
                { 
                    TargetId = targetId, 
                    TargetType = "MenuItem",
                    AverageRating = 0,
                    ReviewCount = 0,
                    LastUpdated = DateTime.UtcNow
                };
            }

        default:
            return new RatingResponse { TargetId = targetId, TargetType = targetType };
    }
}

    public async Task UpdateRatingsAsync()
    {
        // Обновляем рейтинги всех ресторанов
        var restaurantRatings = await _context.RestaurantReviews
            .GroupBy(r => r.RestaurantId)
            .Select(g => new
            {
                RestaurantId = g.Key,
                AverageRating = g.Average(r => r.Rating),
                ReviewCount = g.Count()
            })
            .ToListAsync();

        foreach (var rating in restaurantRatings)
        {
            var existingRating = await _context.RestaurantRatings
                .FirstOrDefaultAsync(r => r.RestaurantId == rating.RestaurantId);

            if (existingRating == null)
            {
                _context.RestaurantRatings.Add(new RestaurantRating
                {
                    RestaurantId = rating.RestaurantId,
                    AverageRating = rating.AverageRating,
                    ReviewCount = rating.ReviewCount
                });
            }
            else
            {
                existingRating.AverageRating = rating.AverageRating;
                existingRating.ReviewCount = rating.ReviewCount;
                existingRating.LastUpdated = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();
    }

    private async Task UpdateRatingForTargetAsync(Guid targetId, string targetType)
{
    Console.WriteLine($"🔢 Updating rating for {targetType} {targetId}");

    try
    {
        switch (targetType.ToLower())
        {
            case "restaurant":
                var restaurantStats = await _context.RestaurantReviews
                    .Where(r => r.RestaurantId == targetId)
                    .GroupBy(r => r.RestaurantId)
                    .Select(g => new { Average = g.Average(r => r.Rating), Count = g.Count() })
                    .FirstOrDefaultAsync();

                Console.WriteLine($"   Restaurant stats: Average={restaurantStats?.Average:F2}, Count={restaurantStats?.Count}");

                if (restaurantStats != null)
                {
                    var existingRating = await _context.RestaurantRatings
                        .FirstOrDefaultAsync(r => r.RestaurantId == targetId);

                    if (existingRating == null)
                    {
                        var newRating = new RestaurantRating
                        {
                            RestaurantId = targetId,
                            AverageRating = restaurantStats.Average,
                            ReviewCount = restaurantStats.Count,
                            LastUpdated = DateTime.UtcNow
                        };
                        _context.RestaurantRatings.Add(newRating);
                        Console.WriteLine($"   ✅ Created new rating: {restaurantStats.Average:F2}");
                    }
                    else
                    {
                        existingRating.AverageRating = restaurantStats.Average;
                        existingRating.ReviewCount = restaurantStats.Count;
                        existingRating.LastUpdated = DateTime.UtcNow;
                        _context.RestaurantRatings.Update(existingRating);
                        Console.WriteLine($"   ✅ Updated existing rating: {restaurantStats.Average:F2}");
                    }

                    await _context.SaveChangesAsync();
                }
                break;

            case "menuitem":
                var menuItemStats = await _context.MenuItemReviews
                    .Where(r => r.MenuItemId == targetId)
                    .GroupBy(r => r.MenuItemId)
                    .Select(g => new { Average = g.Average(r => r.Rating), Count = g.Count() })
                    .FirstOrDefaultAsync();

                if (menuItemStats != null)
                {
                    var existingRating = await _context.MenuItemRatings
                        .FirstOrDefaultAsync(r => r.MenuItemId == targetId);

                    if (existingRating == null)
                    {
                        _context.MenuItemRatings.Add(new MenuItemRating
                        {
                            MenuItemId = targetId,
                            AverageRating = menuItemStats.Average,
                            ReviewCount = menuItemStats.Count,
                            LastUpdated = DateTime.UtcNow
                        });
                    }
                    else
                    {
                        existingRating.AverageRating = menuItemStats.Average;
                        existingRating.ReviewCount = menuItemStats.Count;
                        existingRating.LastUpdated = DateTime.UtcNow;
                    }

                    await _context.SaveChangesAsync();
                }
                break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error updating rating: {ex.Message}");
    }
}

}