using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using NomNomGo.MenuOrderService.Application.DTOs;
using NomNomGo.MenuOrderService.Domain.Entities;
using NomNomGo.MenuOrderService.Infrastructure.Data;
using RecommendationService.Application.DTOs;
using RatingResponse = NomNomGo.MenuOrderService.Application.DTOs.RatingResponse;

namespace NomNomGo.MenuOrderService.Application.Services;

public class MenuService : IMenuService
{
    private readonly MenuOrderDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MenuService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _recommendationServiceUrl;

    public MenuService(
        MenuOrderDbContext context, 
        IHttpClientFactory httpClientFactory, 
        ILogger<MenuService> logger,
        IConfiguration configuration)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _configuration = configuration;
        
        // Получаем URL сервиса рекомендаций из конфигурации
        _recommendationServiceUrl = _configuration["Services:RecommendationService"] 
            ?? "http://localhost:5201";
        
        _logger.LogInformation($"Using RecommendationService URL: {_recommendationServiceUrl}");
    }

    public async Task<IEnumerable<MenuItemResponse>> GetMenuByRestaurantAsync(Guid restaurantId)
    {
        var menuItems = await _context.MenuItems
            .Include(m => m.Restaurant)
            .Include(m => m.Category)
            .Where(m => m.RestaurantId == restaurantId && m.IsAvailable)
            .OrderBy(m => m.Category.Name)
            .ThenBy(m => m.Name)
            .ToListAsync();

        _logger.LogInformation($"Found {menuItems.Count} menu items for restaurant {restaurantId}");
        var responses = new List<MenuItemResponse>();

        foreach (var item in menuItems)
        {
            var rating = await GetMenuItemRatingAsync(item.Id);
            responses.Add(new MenuItemResponse
            {
                Id = item.Id,
                RestaurantId = item.RestaurantId,
                RestaurantName = item.Restaurant.Name,
                CategoryId = item.CategoryId,
                CategoryName = item.Category.Name,
                Name = item.Name,
                Description = item.Description,
                Price = item.Price,
                IsAvailable = item.IsAvailable,
                ImageUrl = item.ImageUrl,
                AverageRating = rating.AverageRating,
                ReviewCount = rating.ReviewCount
            });
        }

        return responses;
    }

    public async Task<IEnumerable<CategoryResponse>> GetCategoriesAsync()
    {
        var categories = await _context.Categories
            .Include(c => c.MenuItems)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return categories.Select(c => new CategoryResponse
        {
            Id = c.Id,
            Name = c.Name,
            ItemsCount = c.MenuItems.Count(mi => mi.IsAvailable)
        });
    }

    public async Task<MenuItemResponse?> GetMenuItemByIdAsync(Guid id)
    {
        var item = await _context.MenuItems
            .Include(m => m.Restaurant)
            .Include(m => m.Category)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (item == null) return null;

        var rating = await GetMenuItemRatingAsync(item.Id);

        return new MenuItemResponse
        {
            Id = item.Id,
            RestaurantId = item.RestaurantId,
            RestaurantName = item.Restaurant.Name,
            CategoryId = item.CategoryId,
            CategoryName = item.Category.Name,
            Name = item.Name,
            Description = item.Description,
            Price = item.Price,
            IsAvailable = item.IsAvailable,
            ImageUrl = item.ImageUrl,
            AverageRating = rating.AverageRating,
            ReviewCount = rating.ReviewCount
        };
    }

    public async Task<MenuItemResponse> CreateMenuItemAsync(CreateMenuItemRequest request)
    {
        var menuItem = new MenuItem
        {
            RestaurantId = request.RestaurantId,
            CategoryId = request.CategoryId,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            ImageUrl = request.ImageUrl,
            IsAvailable = true
        };

        _context.MenuItems.Add(menuItem);
        await _context.SaveChangesAsync();

        // Загружаем связанные данные
        await _context.Entry(menuItem)
            .Reference(m => m.Restaurant)
            .LoadAsync();
        
        await _context.Entry(menuItem)
            .Reference(m => m.Category)
            .LoadAsync();

        return new MenuItemResponse
        {
            Id = menuItem.Id,
            RestaurantId = menuItem.RestaurantId,
            RestaurantName = menuItem.Restaurant.Name,
            CategoryId = menuItem.CategoryId,
            CategoryName = menuItem.Category.Name,
            Name = menuItem.Name,
            Description = menuItem.Description,
            Price = menuItem.Price,
            IsAvailable = menuItem.IsAvailable,
            ImageUrl = menuItem.ImageUrl,
            AverageRating = 0,
            ReviewCount = 0
        };
    }

    public async Task<bool> UpdateMenuItemAsync(Guid id, CreateMenuItemRequest request)
    {
        var menuItem = await _context.MenuItems.FindAsync(id);
        if (menuItem == null) return false;

        // Сохраняем изменение цены
        if (menuItem.Price != request.Price)
        {
            _context.MenuItemChanges.Add(new MenuItemChange
            {
                MenuItemId = id,
                OldPrice = menuItem.Price,
                NewPrice = request.Price,
                ChangedAt = DateTime.UtcNow
            });
        }

        menuItem.CategoryId = request.CategoryId;
        menuItem.Name = request.Name;
        menuItem.Description = request.Description;
        menuItem.Price = request.Price;
        menuItem.ImageUrl = request.ImageUrl;
        menuItem.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteMenuItemAsync(Guid id)
    {
        var menuItem = await _context.MenuItems.FindAsync(id);
        if (menuItem == null) return false;

        menuItem.IsAvailable = false;
        menuItem.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    private async Task<(double AverageRating, int ReviewCount)> GetMenuItemRatingAsync(Guid menuItemId)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(5); // Устанавливаем таймаут
            
            var url = $"{_recommendationServiceUrl}/api/reviews/rating/{menuItemId}/menuitem";
            _logger.LogDebug($"Requesting rating from: {url}");
            
            var response = await httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var ratingData = await response.Content.ReadFromJsonAsync<RatingResponse>();
                _logger.LogDebug($"Rating for {menuItemId}: {ratingData?.AverageRating ?? 0} ({ratingData?.ReviewCount ?? 0} reviews)");
                return (ratingData?.AverageRating ?? 0, ratingData?.ReviewCount ?? 0);
            }
            else
            {
                _logger.LogWarning($"Failed to get rating for {menuItemId}: {response.StatusCode}");
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning($"Network error getting menu item rating for {menuItemId}: {ex.Message}");
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning($"Timeout getting menu item rating for {menuItemId}: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting menu item rating for {menuItemId}: {ex.Message}");
        }

        return (0, 0);
    }
}