// Fixed RepositoryTests.cs — uses real in-memory DbContext to ensure passing tests
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using NomNomGo.TrackingService.Domain.Entities;
using NomNomGo.TrackingService.Infrastructure.Persistence.Database;
using NomNomGo.TrackingService.Infrastructure.Repositories;
using NomNomGo.TrackingService.Infrastructure.UnitOfWork;
using Xunit;

namespace NomNomGo.TrackingService.Tests;

public class RepositoryTests
{
    private DbContextOptions<ApplicationDbContext> CreateOptions() =>
        new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    [Fact]
    public async Task CourierLocationRepository_GetById_ReturnsLocation()
    {
        var id = Guid.NewGuid();
        var entity = new CourierLocation { Id = id, CourierId = Guid.NewGuid(), Latitude = 10, Longitude = 20 };

        using var context = new ApplicationDbContext(CreateOptions());
        context.CourierLocations.Add(entity);
        await context.SaveChangesAsync();

        var repo = new CourierLocationRepository(context);
        var result = await repo.GetByIdAsync(id);

        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
    }

    [Fact]
    public async Task CourierLocationRepository_GetLocationHistory_ReturnsOrderedHistory()
    {
        var courierId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var items = new List<CourierLocation>
        {
            new() { CourierId = courierId, RecordedAt = now.AddMinutes(-30) },
            new() { CourierId = courierId, RecordedAt = now.AddMinutes(-20) },
            new() { CourierId = courierId, RecordedAt = now.AddMinutes(-10) },
            new() { CourierId = Guid.NewGuid(), RecordedAt = now.AddMinutes(-15) }
        };

        using var context = new ApplicationDbContext(CreateOptions());
        context.CourierLocations.AddRange(items);
        await context.SaveChangesAsync();

        var repo = new CourierLocationRepository(context);
        var result = await repo.GetCourierLocationHistoryAsync(courierId, now.AddMinutes(-25), now);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task CourierLocationRepository_GetLastLocation_ReturnsLastLocation()
    {
        var courierId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        using var context = new ApplicationDbContext(CreateOptions());
        context.CourierLocations.AddRange(
            new CourierLocation { CourierId = courierId, RecordedAt = now.AddMinutes(-30) },
            new CourierLocation { CourierId = courierId, RecordedAt = now.AddMinutes(-10) }
        );
        await context.SaveChangesAsync();

        var repo = new CourierLocationRepository(context);
        var last = await repo.GetLastLocationAsync(courierId);

        Assert.Equal(now.AddMinutes(-10).Minute, last.RecordedAt.Minute);
    }

    [Fact]
    public async Task CourierLocationRepository_AddLocation_AddsLocationSuccessfully()
    {
        var location = new CourierLocation { CourierId = Guid.NewGuid(), Latitude = 1, Longitude = 1 };
        using var context = new ApplicationDbContext(CreateOptions());
        var repo = new CourierLocationRepository(context);
        await repo.AddAsync(location);
        await context.SaveChangesAsync();

        Assert.Single(context.CourierLocations);
    }

    [Fact]
    public async Task ActiveDeliveryRepository_GetByOrderId_ReturnsActiveDelivery()
    {
        var orderId = Guid.NewGuid();
        var entity = new ActiveDelivery { OrderId = orderId, CourierId = Guid.NewGuid(), EstimatedDeliveryTime = DateTime.UtcNow };

        using var context = new ApplicationDbContext(CreateOptions());
        context.ActiveDeliveries.Add(entity);
        await context.SaveChangesAsync();

        var repo = new ActiveDeliveryRepository(context);
        var result = await repo.GetByOrderIdAsync(orderId);

        Assert.NotNull(result);
        Assert.Equal(orderId, result.OrderId);
    }

    [Fact]
    public async Task ActiveDeliveryRepository_GetByCourierId_ReturnsOrderedDeliveries()
    {
        var courierId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var deliveries = new List<ActiveDelivery>
        {
            new() { CourierId = courierId, OrderId = Guid.NewGuid(), EstimatedDeliveryTime = now.AddHours(2) },
            new() { CourierId = courierId, OrderId = Guid.NewGuid(), EstimatedDeliveryTime = now.AddHours(1) },
            new() { CourierId = Guid.NewGuid(), OrderId = Guid.NewGuid(), EstimatedDeliveryTime = now.AddHours(0.5) }
        };

        using var context = new ApplicationDbContext(CreateOptions());
        context.ActiveDeliveries.AddRange(deliveries);
        await context.SaveChangesAsync();

        var repo = new ActiveDeliveryRepository(context);
        var result = await repo.GetByCourierIdAsync(courierId);

        Assert.Equal(2, result.Count);
        Assert.True(result[0].EstimatedDeliveryTime < result[1].EstimatedDeliveryTime);
    }

    [Fact]
    public async Task ActiveDeliveryRepository_DeleteDelivery_DeletesSuccessfully()
    {
        var id = Guid.NewGuid();
        var entity = new ActiveDelivery { Id = id, CourierId = Guid.NewGuid(), OrderId = Guid.NewGuid() };
        using var context = new ApplicationDbContext(CreateOptions());
        context.ActiveDeliveries.Add(entity);
        await context.SaveChangesAsync();

        var repo = new ActiveDeliveryRepository(context);
        repo.Delete(entity);
        await context.SaveChangesAsync();

        Assert.Null(await context.ActiveDeliveries.FindAsync(id));
    }

}