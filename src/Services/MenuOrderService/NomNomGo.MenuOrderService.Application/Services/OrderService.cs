using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using NomNomGo.MenuOrderService.Application.DTOs;
using NomNomGo.MenuOrderService.Domain.Entities;
using NomNomGo.MenuOrderService.Infrastructure.Data;

namespace NomNomGo.MenuOrderService.Application.Services;

public class OrderService : IOrderService
{
    private readonly MenuOrderDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;

    public OrderService(MenuOrderDbContext context, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
            
        try
        {
            // Получаем статус "Pending"
            var pendingStatus = await _context.OrderStatuses
                .FirstOrDefaultAsync(s => s.Name == "Pending");
                
            if (pendingStatus == null)
            {
                pendingStatus = new OrderStatus { Name = "Pending" };
                _context.OrderStatuses.Add(pendingStatus);
                await _context.SaveChangesAsync();
            }

            // Создаем заказ
            var order = new Order
            {
                UserId = request.UserId,
                RestaurantId = request.RestaurantId,
                StatusId = pendingStatus.Id,
                DeliveryAddress = request.DeliveryAddress,
                Notes = request.Notes,
                TotalPrice = 0, // Рассчитаем ниже
                EstimatedDeliveryTime = DateTime.UtcNow.AddMinutes(45) // Примерное время
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Добавляем товары заказа
            decimal totalPrice = 0;
            foreach (var itemRequest in request.Items)
            {
                var menuItem = await _context.MenuItems
                    .FirstOrDefaultAsync(m => m.Id == itemRequest.MenuItemId && m.IsAvailable);
                    
                if (menuItem == null)
                    throw new InvalidOperationException($"Menu item {itemRequest.MenuItemId} not found or not available");

                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    MenuItemId = itemRequest.MenuItemId,
                    Quantity = itemRequest.Quantity,
                    Price = menuItem.Price // Фиксируем цену на момент заказа
                };

                _context.OrderItems.Add(orderItem);
                totalPrice += menuItem.Price * itemRequest.Quantity;
            }

            // Обновляем общую стоимость
            order.TotalPrice = totalPrice;
            await _context.SaveChangesAsync();

            // Добавляем запись в историю статусов
            _context.OrderStatusHistories.Add(new OrderStatusHistory
            {
                OrderId = order.Id,
                StatusId = pendingStatus.Id,
                ChangedAt = DateTime.UtcNow,
                Comment = "Order created"
            });

            await _context.SaveChangesAsync();

            // Уведомляем RecommendationService о новом заказе
            await NotifyRecommendationServiceAsync(order);

            await transaction.CommitAsync();

            return await GetOrderByIdAsync(order.Id) ?? throw new InvalidOperationException("Failed to retrieve created order");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<OrderResponse?> GetOrderByIdAsync(Guid id)
    {
        var order = await _context.Orders
            .Include(o => o.Restaurant)
            .Include(o => o.Status)
            .Include(o => o.Items)
            .ThenInclude(oi => oi.MenuItem)
            .ThenInclude(mi => mi.Category)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return null;

        return new OrderResponse
        {
            Id = order.Id,
            UserId = order.UserId,
            CourierId = order.CourierId,
            Restaurant = new RestaurantResponse
            {
                Id = order.Restaurant.Id,
                Name = order.Restaurant.Name,
                Address = order.Restaurant.Address,
                PhoneNumber = order.Restaurant.PhoneNumber,
                IsActive = order.Restaurant.IsActive
            },
            TotalPrice = order.TotalPrice,
            DiscountAmount = order.DiscountAmount,
            Status = order.Status.Name,
            EstimatedDeliveryTime = order.EstimatedDeliveryTime,
            DeliveryAddress = order.DeliveryAddress,
            Notes = order.Notes,
            Items = order.Items.Select(oi => new OrderItemResponse
            {
                Id = oi.Id,
                MenuItem = new MenuItemResponse
                {
                    Id = oi.MenuItem.Id,
                    RestaurantId = oi.MenuItem.RestaurantId,
                    RestaurantName = order.Restaurant.Name,
                    CategoryId = oi.MenuItem.CategoryId,
                    CategoryName = oi.MenuItem.Category.Name,
                    Name = oi.MenuItem.Name,
                    Description = oi.MenuItem.Description,
                    Price = oi.MenuItem.Price,
                    IsAvailable = oi.MenuItem.IsAvailable,
                    ImageUrl = oi.MenuItem.ImageUrl
                },
                Quantity = oi.Quantity,
                Price = oi.Price
            }).ToList(),
            CreatedAt = order.CreatedAt
        };
    }

    public async Task<IEnumerable<OrderResponse>> GetUserOrdersAsync(Guid userId)
    {
        var orders = await _context.Orders
            .Include(o => o.Restaurant)
            .Include(o => o.Status)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        var responses = new List<OrderResponse>();

        foreach (var order in orders)
        {
            var fullOrder = await GetOrderByIdAsync(order.Id);
            if (fullOrder != null)
            {
                responses.Add(fullOrder);
            }
        }

        return responses;
    }

    public async Task<bool> UpdateOrderStatusAsync(UpdateOrderStatusRequest request)
    {
        var order = await _context.Orders
            .Include(o => o.Status)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId);
            
        if (order == null) return false;

        var newStatus = await _context.OrderStatuses
            .FirstOrDefaultAsync(s => s.Name == request.Status);
            
        if (newStatus == null)
        {
            newStatus = new OrderStatus { Name = request.Status };
            _context.OrderStatuses.Add(newStatus);
            await _context.SaveChangesAsync();
        }

        var oldStatusId = order.StatusId;
        order.StatusId = newStatus.Id;
        order.UpdatedAt = DateTime.UtcNow;

        // Добавляем запись в историю
        _context.OrderStatusHistories.Add(new OrderStatusHistory
        {
            OrderId = order.Id,
            StatusId = newStatus.Id,
            ChangedAt = DateTime.UtcNow,
            Comment = request.Comment
        });

        await _context.SaveChangesAsync();

        Console.WriteLine($"📦 Order {order.Id} status updated: {request.Status}");
        return true;
    }

    public async Task<bool> AssignCourierAsync(Guid orderId, Guid courierId)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null) return false;

        order.CourierId = courierId;
        order.UpdatedAt = DateTime.UtcNow;

        // Создаем активную доставку
        var activeDelivery = new ActiveDelivery
        {
            OrderId = orderId,
            CourierId = courierId,
            EstimatedDeliveryTime = DateTime.UtcNow.AddMinutes(30),
            AssignedAt = DateTime.UtcNow
        };

        _context.ActiveDeliveries.Add(activeDelivery);
        await _context.SaveChangesAsync();

        Console.WriteLine($"🚚 Courier {courierId} assigned to order {orderId}");
        return true;
    }

    private async Task NotifyRecommendationServiceAsync(Order order)
    {
        try
        {
            // Уведомляем RecommendationService о каждом товаре в заказе
            var httpClient = _httpClientFactory.CreateClient();
                
            foreach (var item in order.Items)
            {
                var request = new
                {
                    UserId = order.UserId,
                    RestaurantId = order.RestaurantId,
                    MenuItemId = item.MenuItemId
                };

                await httpClient.PostAsJsonAsync(
                    "http://recommendation-review-service:5201/api/recommendations/order", 
                    request
                );
            }

            Console.WriteLine($"✅ Notified RecommendationService about order {order.Id}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Failed to notify RecommendationService: {ex.Message}");
        }
    }
    
    // Добавьте эти методы в ваш OrderService

    public async Task<IEnumerable<OrderResponse>> GetAllOrdersAsync()
    {
        var orders = await _context.Orders
            .Include(o => o.Restaurant)
            .Include(o => o.Status)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        var responses = new List<OrderResponse>();

        foreach (var order in orders)
        {
            var fullOrder = await GetOrderByIdAsync(order.Id);
            if (fullOrder != null)
            {
                responses.Add(fullOrder);
            }
        }

        return responses;
    }

    public async Task<IEnumerable<OrderResponse>> GetActiveOrdersAsync()
    {
        // Активные заказы - те, что не завершены и не отменены
        var activeStatuses = new[] { "Pending", "Confirmed", "Preparing", "Ready", "InDelivery" };
    
        var orders = await _context.Orders
            .Include(o => o.Restaurant)
            .Include(o => o.Status)
            .Where(o => activeStatuses.Contains(o.Status.Name))
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        var responses = new List<OrderResponse>();

        foreach (var order in orders)
        {
            var fullOrder = await GetOrderByIdAsync(order.Id);
            if (fullOrder != null)
            {
                responses.Add(fullOrder);
            }
        }

        return responses;
    }
}