using Microsoft.EntityFrameworkCore;
using RecommendationService.Domain.Entities;

namespace RecommendationService.Infrastructure.Data;

public class RecommendationReviewDbContext : DbContext
{
    public RecommendationReviewDbContext(DbContextOptions<RecommendationReviewDbContext> options) : base(options) { }

    // Entities
    public DbSet<ReviewSentiment> ReviewSentiments => Set<ReviewSentiment>();
    public DbSet<RestaurantReview> RestaurantReviews => Set<RestaurantReview>();
    public DbSet<MenuItemReview> MenuItemReviews => Set<MenuItemReview>();
    public DbSet<CourierReview> CourierReviews => Set<CourierReview>();
    public DbSet<RestaurantRating> RestaurantRatings => Set<RestaurantRating>();
    public DbSet<MenuItemRating> MenuItemRatings => Set<MenuItemRating>();
    public DbSet<CourierRating> CourierRatings => Set<CourierRating>();
    public DbSet<UserOrderHistory> UserOrderHistories => Set<UserOrderHistory>();
    public DbSet<PopularMenuItem> PopularMenuItems => Set<PopularMenuItem>();
    public DbSet<MLModel> MLModels => Set<MLModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Индексы для производительности
        modelBuilder.Entity<RestaurantReview>()
            .HasIndex(r => r.RestaurantId);
            
        modelBuilder.Entity<MenuItemReview>()
            .HasIndex(r => r.MenuItemId);
            
        modelBuilder.Entity<CourierReview>()
            .HasIndex(r => r.CourierId);
            
        modelBuilder.Entity<UserOrderHistory>()
            .HasIndex(h => new { h.UserId, h.OrderDate });
            
        // Связи
        modelBuilder.Entity<RestaurantReview>()
            .HasOne(r => r.Sentiment)
            .WithMany()
            .HasForeignKey(r => r.SentimentId);
            
        modelBuilder.Entity<MenuItemReview>()
            .HasOne(r => r.Sentiment)
            .WithMany()
            .HasForeignKey(r => r.SentimentId);
            
        modelBuilder.Entity<CourierReview>()
            .HasOne(r => r.Sentiment)
            .WithMany()
            .HasForeignKey(r => r.SentimentId);

        base.OnModelCreating(modelBuilder);
    }
}
