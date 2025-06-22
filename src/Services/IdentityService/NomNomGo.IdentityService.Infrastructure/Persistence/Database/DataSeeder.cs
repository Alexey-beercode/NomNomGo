using Microsoft.EntityFrameworkCore;
using NomNomGo.IdentityService.Domain.Entities;
using NomNomGo.IdentityService.Domain.Entities.Relationships;
using Org.BouncyCastle.Crypto.Generators;

namespace NomNomGo.IdentityService.Infrastructure.Persistence.Database;

 public static class DataSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // Создаём роли если их нет
            await SeedRolesAsync(context);
            
            // Создаём админа если его нет
            await SeedAdminAsync(context);
            
            await context.SaveChangesAsync();
        }

        private static async Task SeedRolesAsync(ApplicationDbContext context)
        {
            // Проверяем есть ли роли
            if (await context.Roles.AnyAsync())
                return;

            var roles = new[]
            {
                new Role
                {
                    Id = Guid.NewGuid(),
                    Name = "User",
                },
                new Role
                {
                    Id = Guid.NewGuid(),
                    Name = "Admin",
                }
            };

            await context.Roles.AddRangeAsync(roles);
        }

        private static async Task SeedAdminAsync(ApplicationDbContext context)
        {
            // Проверяем есть ли админ
            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            if (adminRole == null) return;

            var adminExists = await context.Users
                .AnyAsync(u => u.Email == "admin@nomnom.local");
            
            if (adminExists) return;

            // Создаём админа
            var adminUser = new User
            {
                Id = Guid.NewGuid(),
                Email = "admin@nomnom.local",
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"), // Хешируем пароль
                PhoneNumber = "+375447777777",
                IsBlocked = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await context.Users.AddAsync(adminUser);
            await context.SaveChangesAsync(); // Сохраняем пользователя сначала

            // Назначаем роль админа
            var userRole = new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = adminUser.Id,
                RoleId = adminRole.Id
            };

            await context.UserRoles.AddAsync(userRole);
        }
    }