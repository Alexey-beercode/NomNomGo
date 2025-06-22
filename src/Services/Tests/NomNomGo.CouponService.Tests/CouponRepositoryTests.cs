// CouponRepositoryTests.cs

using Microsoft.EntityFrameworkCore;
using NomNomGo.CouponService.Domain.Entities;
using NomNomGo.CouponService.Infrastructure.Persistence.Database;
using NomNomGo.CouponService.Infrastructure.Repositories;
using NomNomGo.CouponService.Infrastructure.UnitOfWork;

namespace NomNomGo.CouponService.Tests;

public class CouponRepositoryTests
{
    private DbContextOptions<ApplicationDbContext> CreateOptions() =>
        new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    [Fact]
    public async Task CouponTypeRepository_GetByNameAsync_ReturnsCorrectType()
    {
        var options = CreateOptions();
        var type = new CouponType { Name = "Unique" };

        using var context = new ApplicationDbContext(options);
        context.CouponTypes.Add(type);
        await context.SaveChangesAsync();

        var repo = new CouponTypeRepository(context);
        var result = await repo.GetByNameAsync("Unique");
        Assert.NotNull(result);
        Assert.Equal("Unique", result!.Name);
    }

    [Fact]
    public async Task CouponRepository_GetByCodeAsync_ReturnsCouponWithType()
    {
        var options = CreateOptions();
        var type = new CouponType { Name = "General" };
        var coupon = new Coupon { Code = "SAVE10", CouponType = type, Discount = 10 };

        using var context = new ApplicationDbContext(options);
        context.CouponTypes.Add(type);
        context.Coupons.Add(coupon);
        await context.SaveChangesAsync();

        var repo = new CouponRepository(context);
        var result = await repo.GetByCodeAsync("SAVE10");
        Assert.NotNull(result);
        Assert.Equal("SAVE10", result!.Code);
        Assert.Equal("General", result.CouponType.Name);
    }

    [Fact]
    public async Task CouponRepository_GetByCodeAsync_ReturnsNullIfNotFound()
    {
        var options = CreateOptions();
        using var context = new ApplicationDbContext(options);
        var repo = new CouponRepository(context);
        var result = await repo.GetByCodeAsync("NOEXIST");
        Assert.Null(result);
    }

    [Fact]
    public async Task CouponRepository_GetActiveCouponsAsync_ReturnsOnlyActive()
    {
        var options = CreateOptions();
        using var context = new ApplicationDbContext(options);
        context.Coupons.AddRange(
            new Coupon { Code = "ACTIVE", IsActive = true },
            new Coupon { Code = "EXPIRED", IsActive = false }
        );
        await context.SaveChangesAsync();

        var repo = new CouponRepository(context);
        var result = await repo.GetActiveCouponsAsync();
        Assert.Single(result);
        Assert.Equal("ACTIVE", result[0].Code);
    }

    [Fact]
    public async Task CouponUsageRepository_GetByUserIdAsync_ReturnsCorrectUsages()
    {
        var options = CreateOptions();
        var userId = Guid.NewGuid();
        var usage = new CouponUsage { UserId = userId, Coupon = new Coupon { Code = "TEST", Discount = 5 }, OrderId = Guid.NewGuid() };

        using var context = new ApplicationDbContext(options);
        context.CouponUsages.Add(usage);
        await context.SaveChangesAsync();

        var repo = new CouponUsageRepository(context);
        var result = await repo.GetByUserIdAsync(userId);
        Assert.Single(result);
    }

    [Fact]
    public async Task CouponUsageRepository_CountCouponUsageAsync_ReturnsCount()
    {
        var options = CreateOptions();
        var userId = Guid.NewGuid();
        var couponId = Guid.NewGuid();

        using var context = new ApplicationDbContext(options);
        context.CouponUsages.AddRange(
            new CouponUsage { UserId = userId, CouponId = couponId, OrderId = Guid.NewGuid() },
            new CouponUsage { UserId = userId, CouponId = couponId, OrderId = Guid.NewGuid() }
        );
        await context.SaveChangesAsync();

        var repo = new CouponUsageRepository(context);
        var count = await repo.CountCouponUsageAsync(couponId, userId);
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task BaseRepository_AddAsync_AddsEntity()
    {
        var options = CreateOptions();
        using var context = new ApplicationDbContext(options);
        var repo = new CouponRepository(context);
        var entity = new Coupon { Code = "ADD", Discount = 5 };
        await repo.AddAsync(entity);
        await context.SaveChangesAsync();
        Assert.Single(context.Coupons);
    }

    [Fact]
    public async Task BaseRepository_GetByIdAsync_ReturnsEntity()
    {
        var options = CreateOptions();
        var id = Guid.NewGuid();
        using var context = new ApplicationDbContext(options);
        context.Coupons.Add(new Coupon { Id = id, Code = "ID", Discount = 5 });
        await context.SaveChangesAsync();
        var repo = new CouponRepository(context);
        var result = await repo.GetByIdAsync(id);
        Assert.NotNull(result);
        Assert.Equal("ID", result!.Code);
    }

    [Fact]
    public async Task BaseRepository_GetAllAsync_ReturnsAllEntities()
    {
        var options = CreateOptions();
        using var context = new ApplicationDbContext(options);
        context.Coupons.AddRange(
            new Coupon { Code = "C1" },
            new Coupon { Code = "C2" }
        );
        await context.SaveChangesAsync();
        var repo = new CouponRepository(context);
        var all = await repo.GetAllAsync();
        Assert.Equal(2, all.Count);
    }

    [Fact]
    public async Task BaseRepository_GetAsync_WithPredicate_FiltersEntities()
    {
        var options = CreateOptions();
        using var context = new ApplicationDbContext(options);
        context.Coupons.AddRange(
            new Coupon { Code = "A", Discount = 5 },
            new Coupon { Code = "B", Discount = 10 }
        );
        await context.SaveChangesAsync();
        var repo = new CouponRepository(context);
        var result = await repo.GetAsync(c => c.Discount > 5);
        Assert.Single(result);
        Assert.Equal("B", result[0].Code);
    }

    [Fact]
    public async Task BaseRepository_Update_UpdatesEntity()
    {
        var options = CreateOptions();
        var id = Guid.NewGuid();
        using var context = new ApplicationDbContext(options);
        var coupon = new Coupon { Id = id, Code = "UPD", Discount = 5 };
        context.Coupons.Add(coupon);
        await context.SaveChangesAsync();
        coupon.Discount = 15;
        var repo = new CouponRepository(context);
        repo.Update(coupon);
        await context.SaveChangesAsync();
        Assert.Equal(15, context.Coupons.Find(id)!.Discount);
    }

    [Fact]
    public async Task BaseRepository_Delete_DeletesEntity()
    {
        var options = CreateOptions();
        var id = Guid.NewGuid();
        using var context = new ApplicationDbContext(options);
        var coupon = new Coupon { Id = id, Code = "DEL" };
        context.Coupons.Add(coupon);
        await context.SaveChangesAsync();
        var repo = new CouponRepository(context);
        repo.Delete(coupon);
        await context.SaveChangesAsync();
        Assert.Null(context.Coupons.Find(id));
    }
}