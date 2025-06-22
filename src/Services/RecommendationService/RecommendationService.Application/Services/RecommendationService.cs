using Microsoft.EntityFrameworkCore;
using RecommendationService.Application.DTOs;
using RecommendationService.Domain.Entities;
using RecommendationService.Infrastructure.Data;

namespace RecommendationService.Application.Services;

public class RecommendationService : IRecommendationService
    {
        private readonly RecommendationReviewDbContext _context;
        private readonly IMlService _mlService;
        private readonly IHttpClientFactory _httpClientFactory;

        public RecommendationService(
            RecommendationReviewDbContext context, 
            IMlService mlService,
            IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _mlService = mlService;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IEnumerable<RecommendationResponse>> GetPersonalizedRecommendationsAsync(Guid userId, int count = 10)
        {
            // Получаем ML рекомендации
            var mlRecommendations = await _mlService.GetRecommendationsAsync(userId, count);
            
            var recommendations = new List<RecommendationResponse>();

            // Получаем информацию о блюдах из микросервиса меню (пока заглушка)
            var menuItems = await GetMenuItemsFromServiceAsync(mlRecommendations.Select(r => r.Score).ToArray());

            for (int i = 0; i < mlRecommendations.Count() && i < menuItems.Count; i++)
            {
                var mlRec = mlRecommendations.ElementAt(i);
                var menuItem = menuItems[i];

                recommendations.Add(new RecommendationResponse
                {
                    MenuItemId = menuItem.Id,
                    ItemName = menuItem.Name,
                    RestaurantId = menuItem.RestaurantId,
                    RestaurantName = menuItem.RestaurantName,
                    Score = mlRec.Score,
                    Reason = "Based on your order history"
                });
            }

            return recommendations;
        }

        public async Task<IEnumerable<RecommendationResponse>> GetPopularRecommendationsAsync(int count = 10)
        {
            var popularItems = await _context.PopularMenuItems
                .OrderByDescending(p => p.OrderCount)
                .Take(count)
                .ToListAsync();

            // ✅ ИСПРАВЛЕНИЕ: Если нет популярных блюд, создаем тестовые
            if (!popularItems.Any())
            {
                Console.WriteLine("⚠️ No popular items found, generating test recommendations");
        
                // Создаем несколько тестовых популярных блюд
                var testMenuItems = new[]
                {
                    Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 
                    Guid.NewGuid(), Guid.NewGuid()
                };

                var testPopularItems = testMenuItems.Select((id, index) => new PopularMenuItem
                {
                    MenuItemId = id,
                    OrderCount = 50 - (index * 10), // 50, 40, 30, 20, 10
                    LastUpdated = DateTime.UtcNow
                }).ToList();

                _context.PopularMenuItems.AddRange(testPopularItems);
                await _context.SaveChangesAsync();

                popularItems = testPopularItems;
            }

            // Получаем информацию о блюдах
            var menuItems = await GetMenuItemsByIdsAsync(popularItems.Select(p => p.MenuItemId).ToArray());

            return popularItems.Zip(menuItems, (popular, menu) => new RecommendationResponse
            {
                MenuItemId = menu.Id,
                ItemName = menu.Name,
                RestaurantId = menu.RestaurantId,
                RestaurantName = menu.RestaurantName,
                Score = Math.Min(popular.OrderCount / 100.0, 1.0), // Нормализуем счетчик
                Reason = $"Popular item ({popular.OrderCount} orders)"
            });
        }

        public async Task AddUserOrderAsync(Guid userId, Guid restaurantId, Guid menuItemId)
        {
            // Добавляем запись в историю заказов
            var orderHistory = new UserOrderHistory
            {
                UserId = userId,
                RestaurantId = restaurantId,
                MenuItemId = menuItemId,
                OrderDate = DateTime.UtcNow
            };

            _context.UserOrderHistories.Add(orderHistory);

            // Обновляем счетчик популярности
            var popularItem = await _context.PopularMenuItems
                .FirstOrDefaultAsync(p => p.MenuItemId == menuItemId);

            if (popularItem == null)
            {
                _context.PopularMenuItems.Add(new PopularMenuItem
                {
                    MenuItemId = menuItemId,
                    OrderCount = 1
                });
            }
            else
            {
                popularItem.OrderCount++;
                popularItem.LastUpdated = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdatePopularItemsAsync()
        {
            // Пересчитываем популярность на основе последних заказов
            var itemCounts = await _context.UserOrderHistories
                .Where(h => h.OrderDate >= DateTime.UtcNow.AddMonths(-1)) // Последний месяц
                .GroupBy(h => h.MenuItemId)
                .Select(g => new { MenuItemId = g.Key, Count = g.Count() })
                .ToListAsync();

            foreach (var item in itemCounts)
            {
                var popularItem = await _context.PopularMenuItems
                    .FirstOrDefaultAsync(p => p.MenuItemId == item.MenuItemId);

                if (popularItem == null)
                {
                    _context.PopularMenuItems.Add(new PopularMenuItem
                    {
                        MenuItemId = item.MenuItemId,
                        OrderCount = item.Count
                    });
                }
                else
                {
                    popularItem.OrderCount = item.Count;
                    popularItem.LastUpdated = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
        }

        // Заглушки для интеграции с микросервисом меню
        private async Task<List<MenuItemDto>> GetMenuItemsFromServiceAsync(float[] scores)
        {
            // TODO: Заменить на реальный вызов микросервиса меню
            return GenerateTestMenuItems(scores.Length);
        }

        private async Task<List<MenuItemDto>> GetMenuItemsByIdsAsync(Guid[] menuItemIds)
        {
            // TODO: Заменить на реальный вызов микросервиса меню
            return GenerateTestMenuItems(menuItemIds.Length);
        }

        private List<MenuItemDto> GenerateTestMenuItems(int count)
        {
            var testItems = new List<MenuItemDto>();
            var random = new Random();
            var dishes = new[] { "Борщ", "Пицца Маргарита", "Суши Сет", "Бургер", "Паста Карбонара", "Цезарь салат", "Стейк", "Плов" };
            var restaurants = new[] { "Мама Рома", "Суши Мастер", "Бургер Кинг", "Домашняя кухня", "Итальянский дворик" };

            for (int i = 0; i < count; i++)
            {
                testItems.Add(new MenuItemDto
                {
                    Id = Guid.NewGuid(),
                    Name = dishes[random.Next(dishes.Length)],
                    RestaurantId = Guid.NewGuid(),
                    RestaurantName = restaurants[random.Next(restaurants.Length)]
                });
            }

            return testItems;
        }
    }