using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using NomNomGo.MenuOrderService.Application.DTOs;
using NomNomGo.MenuOrderService.Domain.Entities;
using NomNomGo.MenuOrderService.Infrastructure.Data;
using RecommendationService.Application.DTOs;
using RatingResponse = NomNomGo.MenuOrderService.Application.DTOs.RatingResponse;

namespace NomNomGo.MenuOrderService.Application.Services;

public class RestaurantService : IRestaurantService
    {
        private readonly MenuOrderDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public RestaurantService(MenuOrderDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IEnumerable<RestaurantResponse>> GetAllRestaurantsAsync()
        {
            var restaurants = await _context.Restaurants
                .Where(r => r.IsActive)
                .OrderBy(r => r.Name)
                .ToListAsync();

            var responses = new List<RestaurantResponse>();

            foreach (var restaurant in restaurants)
            {
                var rating = await GetRestaurantRatingAsync(restaurant.Id);
                responses.Add(new RestaurantResponse
                {
                    Id = restaurant.Id,
                    Name = restaurant.Name,
                    Address = restaurant.Address,
                    PhoneNumber = restaurant.PhoneNumber,
                    IsActive = restaurant.IsActive,
                    AverageRating = rating.AverageRating,
                    ReviewCount = rating.ReviewCount,
                    ImageUrl = restaurant.ImageUrl
                });
            }

            return responses;
        }

        public async Task<RestaurantResponse?> GetRestaurantByIdAsync(Guid id)
        {
            var restaurant = await _context.Restaurants
                .FirstOrDefaultAsync(r => r.Id == id && r.IsActive);

            if (restaurant == null) return null;

            var rating = await GetRestaurantRatingAsync(restaurant.Id);

            return new RestaurantResponse
            {
                Id = restaurant.Id,
                Name = restaurant.Name,
                Address = restaurant.Address,
                PhoneNumber = restaurant.PhoneNumber,
                IsActive = restaurant.IsActive,
                AverageRating = rating.AverageRating,
                ReviewCount = rating.ReviewCount,
                ImageUrl = restaurant.ImageUrl
            };
        }

        public async Task<RestaurantResponse> CreateRestaurantAsync(CreateRestaurantRequest request)
        {
            var restaurant = new Restaurant
            {
                Name = request.Name,
                Address = request.Address,
                PhoneNumber = request.PhoneNumber,
                ImageUrl = request.ImageUrl,
                IsActive = true
            };

            _context.Restaurants.Add(restaurant);
            await _context.SaveChangesAsync();

            return new RestaurantResponse
            {
                Id = restaurant.Id,
                Name = restaurant.Name,
                Address = restaurant.Address,
                PhoneNumber = restaurant.PhoneNumber,
                IsActive = restaurant.IsActive,
                ImageUrl = restaurant.ImageUrl,
                AverageRating = 0,
                ReviewCount = 0
            };
        }

        public async Task<bool> UpdateRestaurantAsync(Guid id, CreateRestaurantRequest request)
        {
            var restaurant = await _context.Restaurants.FindAsync(id);
            if (restaurant == null) return false;

            restaurant.Name = request.Name;
            restaurant.Address = request.Address;
            restaurant.PhoneNumber = request.PhoneNumber;
            restaurant.UpdatedAt = DateTime.UtcNow;
            restaurant.ImageUrl = request.ImageUrl;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteRestaurantAsync(Guid id)
        {
            var restaurant = await _context.Restaurants.FindAsync(id);
            if (restaurant == null) return false;

            restaurant.IsActive = false;
            restaurant.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<(double AverageRating, int ReviewCount)> GetRestaurantRatingAsync(Guid restaurantId)
        {
            try
            {
                // Вызов RecommendationService для получения рейтинга
                var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.GetAsync($"http://recommendation-review-service:5201/api/reviews/rating/{restaurantId}/Restaurant");
                
                if (response.IsSuccessStatusCode)
                {
                    var ratingData = await response.Content.ReadFromJsonAsync<RatingResponse>();
                    return (ratingData?.AverageRating ?? 0, ratingData?.ReviewCount ?? 0);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting restaurant rating: {ex.Message}");
            }

            return (0, 0);
        }
    }