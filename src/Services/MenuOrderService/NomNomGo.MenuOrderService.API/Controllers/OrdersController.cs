using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using NomNomGo.MenuOrderService.API.Hubs;
using NomNomGo.MenuOrderService.Application.DTOs;
using NomNomGo.MenuOrderService.Application.Services;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IHubContext<TrackingHub> _hubContext;
    private readonly ILogger <OrdersController> _logger;

    public OrdersController(IOrderService orderService, IHubContext<TrackingHub> hubContext, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _hubContext = hubContext;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<OrderResponse>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        try
        {
            var order = await _orderService.CreateOrderAsync(request);
            
            // Уведомляем клиента через SignalR
            await _hubContext.Clients.Group($"user_{request.UserId}")
                .SendAsync("OrderCreated", order);
            
            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderResponse>>> GetAllOrders()
    {
        var orders = await _orderService.GetAllOrdersAsync();
        return Ok(orders);
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<OrderResponse>>> GetActiveOrders()
    {
        var orders = await _orderService.GetActiveOrdersAsync();
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderResponse>> GetOrder(Guid id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        return order == null ? NotFound() : Ok(order);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<OrderResponse>>> GetUserOrders(Guid userId)
    {
        var orders = await _orderService.GetUserOrdersAsync(userId);
        return Ok(orders);
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
    {
        _logger.LogInformation("Обновляется статус заказа");
        request.OrderId = id;
        var updated = await _orderService.UpdateOrderStatusAsync(request);
        
        if (updated)
        {
            // Уведомляем через SignalR о смене статуса
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order != null)
            {
                await _hubContext.Clients.Group($"user_{order.UserId}")
                    .SendAsync("OrderStatusUpdated", new { OrderId = id, Status = request.Status });
            }
        }
        
        return updated ? NoContent() : NotFound();
    }

    [HttpPut("{id}/assign-courier/{courierId}")]
    public async Task<IActionResult> AssignCourier(Guid id, Guid courierId)
    {
        var assigned = await _orderService.AssignCourierAsync(id, courierId);
        return assigned ? NoContent() : NotFound();
    }
}