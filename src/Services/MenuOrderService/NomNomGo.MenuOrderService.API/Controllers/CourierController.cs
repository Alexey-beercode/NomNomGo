using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NomNomGo.MenuOrderService.Application.DTOs;
using NomNomGo.MenuOrderService.Application.Services;
using NomNomGo.MenuOrderService.Infrastructure.Data;

[ApiController]
[Route("api/[controller]")]
public class CourierController : ControllerBase
{
    private readonly ITrackingService _trackingService;
    private readonly IOrderService _orderService;
    private readonly MenuOrderDbContext _context;
    private readonly ILogger<CourierController> _logger;

    public CourierController(ITrackingService trackingService, IOrderService orderService, MenuOrderDbContext context, ILogger<CourierController> logger)
    {
        _trackingService = trackingService;
        _orderService = orderService;
        _context = context;
        _logger = logger;
    }

    [HttpGet("active-orders/{courierId}")]
    public async Task<ActionResult<IEnumerable<OrderResponse>>> GetActiveOrders(Guid courierId)
    {
        // Получаем активные заказы курьера
        var activeDeliveries = await _context.ActiveDeliveries
            .Include(ad => ad.Order)
            .ThenInclude(o => o.Restaurant)
            .Where(ad => ad.CourierId == courierId)
            .ToListAsync();

        var orders = new List<OrderResponse>();
        foreach (var delivery in activeDeliveries)
        {
            var order = await _orderService.GetOrderByIdAsync(delivery.OrderId);
            if (order != null) orders.Add(order);
        }

        return Ok(orders);
    }

    [HttpPost("update-status")]
    public async Task<IActionResult> UpdateOrderStatus([FromBody] CourierStatusUpdate request)
    {
        _logger.LogInformation("Обновляется статус курьера");
        var statusRequest = new UpdateOrderStatusRequest
        {
            OrderId = request.OrderId,
            Status = request.Status,
            Comment = $"Updated by courier {request.CourierId}"
        };

        var updated = await _orderService.UpdateOrderStatusAsync(statusRequest);
        return updated ? Ok() : BadRequest();
    }
}

public class CourierStatusUpdate
{
    public Guid CourierId { get; set; }
    public Guid OrderId { get; set; }
    public string Status { get; set; } = string.Empty; // "Ready", "InDelivery", "Delivered"
}