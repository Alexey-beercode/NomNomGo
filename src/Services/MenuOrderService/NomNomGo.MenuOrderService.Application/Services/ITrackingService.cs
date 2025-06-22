using NomNomGo.MenuOrderService.Application.DTOs;

namespace NomNomGo.MenuOrderService.Application.Services;

public interface ITrackingService
{
    Task UpdateCourierLocationAsync(CourierLocationUpdate locationUpdate);
    Task<TrackingResponse?> GetOrderTrackingAsync(Guid orderId);
    Task<IEnumerable<CourierLocationUpdate>> GetCourierLocationsAsync(Guid courierId, int limit = 10);
}