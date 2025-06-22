using NomNomGo.MenuOrderService.Application.DTOs;

namespace NomNomGo.MenuOrderService.Application.Services;

public interface IRestaurantService
{
    Task<IEnumerable<RestaurantResponse>> GetAllRestaurantsAsync();
    Task<RestaurantResponse?> GetRestaurantByIdAsync(Guid id);
    Task<RestaurantResponse> CreateRestaurantAsync(CreateRestaurantRequest request);
    Task<bool> UpdateRestaurantAsync(Guid id, CreateRestaurantRequest request);
    Task<bool> DeleteRestaurantAsync(Guid id);
}