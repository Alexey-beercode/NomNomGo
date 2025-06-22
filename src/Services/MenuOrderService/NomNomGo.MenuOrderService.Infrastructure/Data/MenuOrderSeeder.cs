using Microsoft.EntityFrameworkCore;
using NomNomGo.MenuOrderService.Domain.Entities;

namespace NomNomGo.MenuOrderService.Infrastructure.Data;

public static class MenuOrderSeeder
    {
        public static async Task SeedAsync(MenuOrderDbContext context)
        {
            // Создаём статусы заказов
            await SeedOrderStatusesAsync(context);
            
            await context.SaveChangesAsync();
        }

        private static async Task SeedOrderStatusesAsync(MenuOrderDbContext context)
        {
            // Проверяем есть ли статусы
            if (await context.OrderStatuses.AnyAsync())
                return;

            var statuses = new[]
            {
                new OrderStatus
                {
                    Id = Guid.NewGuid(),
                    Name = "Pending",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new OrderStatus
                {
                    Id = Guid.NewGuid(),
                    Name = "Confirmed",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new OrderStatus
                {
                    Id = Guid.NewGuid(),
                    Name = "Preparing",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new OrderStatus
                {
                    Id = Guid.NewGuid(),
                    Name = "Ready",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new OrderStatus
                {
                    Id = Guid.NewGuid(),
                    Name = "InDelivery",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new OrderStatus
                {
                    Id = Guid.NewGuid(),
                    Name = "Delivered",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new OrderStatus
                {
                    Id = Guid.NewGuid(),
                    Name = "Cancelled",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            await context.OrderStatuses.AddRangeAsync(statuses);
        }
    }