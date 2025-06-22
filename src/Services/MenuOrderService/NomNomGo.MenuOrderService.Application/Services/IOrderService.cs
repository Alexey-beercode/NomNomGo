using NomNomGo.MenuOrderService.Application.DTOs;

namespace NomNomGo.MenuOrderService.Application.Services;

// Добавьте эти методы в интерфейс IOrderService

public interface IOrderService
{
    Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request);
    Task<OrderResponse?> GetOrderByIdAsync(Guid id);
    Task<IEnumerable<OrderResponse>> GetUserOrdersAsync(Guid userId);
    Task<IEnumerable<OrderResponse>> GetAllOrdersAsync(); // Новый метод
    Task<IEnumerable<OrderResponse>> GetActiveOrdersAsync(); // Новый метод
    Task<bool> UpdateOrderStatusAsync(UpdateOrderStatusRequest request);
    Task<bool> AssignCourierAsync(Guid orderId, Guid courierId);
}