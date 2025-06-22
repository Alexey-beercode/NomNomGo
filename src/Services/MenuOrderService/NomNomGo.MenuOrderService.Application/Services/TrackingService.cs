using Microsoft.EntityFrameworkCore;
using NomNomGo.MenuOrderService.Application.DTOs;
using NomNomGo.MenuOrderService.Domain.Entities;
using NomNomGo.MenuOrderService.Infrastructure.Data;

namespace NomNomGo.MenuOrderService.Application.Services;

public class TrackingService : ITrackingService
{
    private readonly MenuOrderDbContext _context;

    public TrackingService(MenuOrderDbContext context)
    {
        _context = context;
    }

    public async Task UpdateCourierLocationAsync(CourierLocationUpdate locationUpdate)
    {
        // Сохраняем в БД
        var courierLocation = new CourierLocation
        {
            CourierId = locationUpdate.CourierId,
            Latitude = locationUpdate.Latitude,
            Longitude = locationUpdate.Longitude,
            RecordedAt = DateTime.UtcNow
        };

        _context.CourierLocations.Add(courierLocation);

        // Обновляем активные доставки
        var activeDeliveries = await _context.ActiveDeliveries
            .Where(ad => ad.CourierId == locationUpdate.CourierId)
            .ToListAsync();

        foreach (var delivery in activeDeliveries)
        {
            delivery.CurrentLatitude = locationUpdate.Latitude;
            delivery.CurrentLongitude = locationUpdate.Longitude;
        }

        await _context.SaveChangesAsync();
            
        Console.WriteLine($"📍 Updated location for courier {locationUpdate.CourierId}: {locationUpdate.Latitude}, {locationUpdate.Longitude}");
    }

    public async Task<TrackingResponse?> GetOrderTrackingAsync(Guid orderId)
    {
        var activeDelivery = await _context.ActiveDeliveries
            .Include(ad => ad.Order)
            .ThenInclude(o => o.Status)
            .FirstOrDefaultAsync(ad => ad.OrderId == orderId);

        if (activeDelivery == null) return null;

        return new TrackingResponse
        {
            OrderId = activeDelivery.OrderId,
            CourierId = activeDelivery.CourierId,
            CourierLatitude = activeDelivery.CurrentLatitude,
            CourierLongitude = activeDelivery.CurrentLongitude,
            EstimatedDeliveryTime = activeDelivery.EstimatedDeliveryTime,
            Status = activeDelivery.Order.Status.Name
        };
    }

    public async Task<IEnumerable<CourierLocationUpdate>> GetCourierLocationsAsync(Guid courierId, int limit = 10)
    {
        var locations = await _context.CourierLocations
            .Where(cl => cl.CourierId == courierId)
            .OrderByDescending(cl => cl.RecordedAt)
            .Take(limit)
            .ToListAsync();

        return locations.Select(cl => new CourierLocationUpdate
        {
            CourierId = cl.CourierId,
            Latitude = cl.Latitude,
            Longitude = cl.Longitude
        });
    }
}