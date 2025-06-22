using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using NomNomGo.MenuOrderService.API.Hubs;
using NomNomGo.MenuOrderService.Application.DTOs;
using NomNomGo.MenuOrderService.Application.Services;

namespace NomNomGo.MenuOrderService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TrackingController : ControllerBase
{
    private readonly ITrackingService _trackingService;
    private readonly IHubContext<TrackingHub> _hubContext;
    private readonly ILogger<TrackingController> _logger;

    public TrackingController(ITrackingService trackingService, IHubContext<TrackingHub> hubContext, ILogger<TrackingController> logger)
    {
        _trackingService = trackingService;
        _hubContext = hubContext;
        _logger = logger;
    }

    [HttpPost("location")]
    public async Task<IActionResult> UpdateCourierLocation([FromBody] CourierLocationUpdate locationUpdate)
    {
       
        await _trackingService.UpdateCourierLocationAsync(locationUpdate);
            
        // Транслируем обновление координат через SignalR
        await _hubContext.Clients.Group($"courier_{locationUpdate.CourierId}")
            .SendAsync("LocationUpdated", locationUpdate);
            
        return Ok();
    }

    [HttpGet("order/{orderId}")]
    public async Task<ActionResult<TrackingResponse>> GetOrderTracking(Guid orderId)
    {
        var tracking = await _trackingService.GetOrderTrackingAsync(orderId);
        return tracking == null ? NotFound() : Ok(tracking);
    }

    [HttpGet("courier/{courierId}/locations")]
    public async Task<ActionResult<IEnumerable<CourierLocationUpdate>>> GetCourierLocations(Guid courierId, int limit = 10)
    {
        var locations = await _trackingService.GetCourierLocationsAsync(courierId, limit);
        return Ok(locations);
    }
}