using NomNomGo.MenuOrderService.Application.DTOs;

namespace NomNomGo.MenuOrderService.Application.Services;

public interface IMenuService
{
    Task<IEnumerable<MenuItemResponse>> GetMenuByRestaurantAsync(Guid restaurantId);
    Task<IEnumerable<CategoryResponse>> GetCategoriesAsync();
    Task<MenuItemResponse?> GetMenuItemByIdAsync(Guid id);
    Task<MenuItemResponse> CreateMenuItemAsync(CreateMenuItemRequest request);
    Task<bool> UpdateMenuItemAsync(Guid id, CreateMenuItemRequest request);
    Task<bool> DeleteMenuItemAsync(Guid id);
}