using Microsoft.EntityFrameworkCore;
using RecommendationService.Application.Services;
using RecommendationService.Domain.Entities;
using RecommendationService.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database - ВАЖНО: Оставляем Scoped
builder.Services.AddDbContext<RecommendationReviewDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// HTTP Client для интеграции с другими микросервисами
builder.Services.AddHttpClient();

// Services - ВАЖНО: Меняем MLService на Scoped вместо Singleton
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IRecommendationService, RecommendationService.Application.Services.RecommendationService>();
builder.Services.AddScoped<IMlService, MlService>(); // ✅ SCOPED вместо SINGLETON

builder.Services.AddCors(options =>
{
    // ПОЛНОСТЬЮ ОТКРЫТАЯ политика для разработки
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()      // Разрешить ВСЕ домены
            .AllowAnyMethod()        // Разрешить ВСЕ методы (GET, POST, PUT, DELETE, OPTIONS)
            .AllowAnyHeader();       // Разрешить ВСЕ заголовки
    });

    // Политика для разработки с credentials (если нужны cookies)
    options.AddPolicy("Development", policy =>
    {
        policy.SetIsOriginAllowed(origin => true)  // Разрешить ВСЕ origins
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });

    // Политика для продакшена (когда будете готовы)
    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",
                "https://localhost:4200",
                "http://127.0.0.1:4200",
                "https://127.0.0.1:4200",
                "capacitor://localhost",
                "ionic://localhost",
                "http://localhost",
                "https://localhost"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowAll");
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

// Seed data (для тестирования)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<RecommendationReviewDbContext>();
    
    // Создаем БД если не существует
    await context.Database.EnsureCreatedAsync();
    
    await SeedTestDataAsync(context);
}

app.Run();

// ===== SEED TEST DATA (без изменений) =====
async Task SeedTestDataAsync(RecommendationReviewDbContext context)
{
    if (!await context.ReviewSentiments.AnyAsync())
    {
        context.ReviewSentiments.AddRange(
            new ReviewSentiment { Name = "Positive" },
            new ReviewSentiment { Name = "Negative" },
            new ReviewSentiment { Name = "Neutral" }
        );
        await context.SaveChangesAsync();
    }

    if (!await context.UserOrderHistories.AnyAsync())
    {
        var random = new Random();
        var testUsers = Enumerable.Range(1, 10).Select(_ => Guid.NewGuid()).ToArray();
        var testRestaurants = Enumerable.Range(1, 5).Select(_ => Guid.NewGuid()).ToArray();
        var testMenuItems = Enumerable.Range(1, 20).Select(_ => Guid.NewGuid()).ToArray();

        var orderHistories = new List<UserOrderHistory>();
        
        // Генерируем 100 тестовых заказов
        for (int i = 0; i < 100; i++)
        {
            orderHistories.Add(new UserOrderHistory
            {
                UserId = testUsers[random.Next(testUsers.Length)],
                RestaurantId = testRestaurants[random.Next(testRestaurants.Length)],
                MenuItemId = testMenuItems[random.Next(testMenuItems.Length)],
                OrderDate = DateTime.UtcNow.AddDays(-random.Next(30))
            });
        }

        context.UserOrderHistories.AddRange(orderHistories);
        await context.SaveChangesAsync();

        // Создаем тестовые отзывы
        var positiveComments = new[] {
            "Отличная еда!",
            "Быстрая доставка",
            "Очень вкусно",
            "Рекомендую!"
        };

        var negativeComments = new[] {
            "Ужасное качество",
            "Долго ждали",
            "Холодная еда",
            "Не понравилось"
        };

        var positiveSentiment = await context.ReviewSentiments.FirstAsync(s => s.Name == "Positive");
        var negativeSentiment = await context.ReviewSentiments.FirstAsync(s => s.Name == "Negative");

        var reviews = new List<RestaurantReview>();
        
        for (int i = 0; i < 50; i++)
        {
            var isPositive = random.NextDouble() > 0.3; // 70% положительных отзывов
            reviews.Add(new RestaurantReview
            {
                UserId = testUsers[random.Next(testUsers.Length)],
                RestaurantId = testRestaurants[random.Next(testRestaurants.Length)],
                Rating = isPositive ? random.Next(4, 6) : random.Next(1, 3),
                Comment = isPositive 
                    ? positiveComments[random.Next(positiveComments.Length)]
                    : negativeComments[random.Next(negativeComments.Length)],
                SentimentId = isPositive ? positiveSentiment.Id : negativeSentiment.Id
            });
        }

        context.RestaurantReviews.AddRange(reviews);
        await context.SaveChangesAsync();

        Console.WriteLine("✅ Test data seeded successfully!");
        Console.WriteLine($"   - {orderHistories.Count} order histories");
        Console.WriteLine($"   - {reviews.Count} restaurant reviews");
    }
}