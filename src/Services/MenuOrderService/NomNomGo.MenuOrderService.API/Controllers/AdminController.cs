using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NomNomGo.MenuOrderService.Domain.Entities;
using NomNomGo.MenuOrderService.Infrastructure.Data;

namespace NomNomGo.MenuOrderService.API.Controllers;

[ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly MenuOrderDbContext _context;

        public AdminController(MenuOrderDbContext context)
        {
            _context = context;
        }

        [HttpPost("seed-data")]
        public async Task<IActionResult> SeedTestData()
        {
            // Создаем тестовые данные для демонстрации
            if (!await _context.OrderStatuses.AnyAsync())
            {
                var statuses = new[]
                {
                    new OrderStatus { Name = "Pending" },
                    new OrderStatus { Name = "Confirmed" },
                    new OrderStatus { Name = "Preparing" },
                    new OrderStatus { Name = "Ready" },
                    new OrderStatus { Name = "InDelivery" },
                    new OrderStatus { Name = "Delivered" },
                    new OrderStatus { Name = "Cancelled" }
                };

                _context.OrderStatuses.AddRange(statuses);
                await _context.SaveChangesAsync();
            }

            if (!await _context.ParsingStatuses.AnyAsync())
            {
                var parsingStatuses = new[]
                {
                    new ParsingStatus { Name = "Pending" },
                    new ParsingStatus { Name = "InProgress" },
                    new ParsingStatus { Name = "Completed" },
                    new ParsingStatus { Name = "Failed" }
                };

                _context.ParsingStatuses.AddRange(parsingStatuses);
                await _context.SaveChangesAsync();
            }

            if (!await _context.Categories.AnyAsync())
            {
                var categories = new[]
                {
                    new Category { Name = "Супы" },
                    new Category { Name = "Горячие блюда" },
                    new Category { Name = "Салаты" },
                    new Category { Name = "Десерты" },
                    new Category { Name = "Напитки" },
                    new Category { Name = "Пицца" },
                    new Category { Name = "Суши" },
                    new Category { Name = "Бургеры" }
                };

                _context.Categories.AddRange(categories);
                await _context.SaveChangesAsync();
            }

            if (!await _context.Restaurants.AnyAsync())
            {
                var restaurants = new[]
                {
                    new Restaurant { Name = "Мама Рома", Address = "ул. Ленина, 15", PhoneNumber = "+7 (800) 123-45-67" },
                    new Restaurant { Name = "Суши Мастер", Address = "пр. Мира, 22", PhoneNumber = "+7 (800) 234-56-78" },
                    new Restaurant { Name = "Бургер Кинг", Address = "ул. Пушкина, 8", PhoneNumber = "+7 (800) 345-67-89" },
                    new Restaurant { Name = "Домашняя кухня", Address = "ул. Гагарина, 33", PhoneNumber = "+7 (800) 456-78-90" },
                    new Restaurant { Name = "Итальянский дворик", Address = "ул. Советская, 12", PhoneNumber = "+7 (800) 567-89-01" }
                };

                _context.Restaurants.AddRange(restaurants);
                await _context.SaveChangesAsync();

                // Добавляем тестовое меню
                var categories = await _context.Categories.ToListAsync();
                var menuItems = new List<MenuItem>();

                foreach (var restaurant in restaurants)
                {
                    // Каждому ресторану добавляем 5-10 блюд
                    var random = new Random();
                    var itemCount = random.Next(5, 11);
                    
                    for (int i = 0; i < itemCount; i++)
                    {
                        var category = categories[random.Next(categories.Count)];
                        menuItems.Add(new MenuItem
                        {
                            RestaurantId = restaurant.Id,
                            CategoryId = category.Id,
                            Name = $"{category.Name} {restaurant.Name} №{i + 1}",
                            Description = $"Отличное блюдо от {restaurant.Name}",
                            Price = random.Next(200, 1500),
                            IsAvailable = true
                        });
                    }
                }

                _context.MenuItems.AddRange(menuItems);
                await _context.SaveChangesAsync();
            }

            return Ok("Test data seeded successfully!");
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var stats = new
            {
                Restaurants = await _context.Restaurants.CountAsync(),
                MenuItems = await _context.MenuItems.CountAsync(),
                Orders = await _context.Orders.CountAsync(),
                Categories = await _context.Categories.CountAsync(),
                ActiveDeliveries = await _context.ActiveDeliveries.CountAsync()
            };

            return Ok(stats);
        }
    }