using Microsoft.EntityFrameworkCore;
using NomNomGo.MenuOrderService.Domain.Entities;

namespace NomNomGo.MenuOrderService.Infrastructure.Data;

public class MenuOrderDbContext : DbContext
    {
        public MenuOrderDbContext(DbContextOptions<MenuOrderDbContext> options) : base(options)
        {
        }

        // Restaurants & Menu
        public DbSet<Restaurant> Restaurants => Set<Restaurant>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<MenuItem> MenuItems => Set<MenuItem>();
        public DbSet<MenuItemChange> MenuItemChanges => Set<MenuItemChange>();

        // Orders
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<OrderStatus> OrderStatuses => Set<OrderStatus>();
        public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();

        // Tracking
        public DbSet<CourierLocation> CourierLocations => Set<CourierLocation>();
        public DbSet<ActiveDelivery> ActiveDeliveries => Set<ActiveDelivery>();

        // Parsing
        public DbSet<ParsingStatus> ParsingStatuses => Set<ParsingStatus>();
        public DbSet<ParsingQueue> ParsingQueues => Set<ParsingQueue>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Relationships
            modelBuilder.Entity<MenuItem>()
                .HasOne(m => m.Restaurant)
                .WithMany(r => r.MenuItems)
                .HasForeignKey(m => m.RestaurantId);

            modelBuilder.Entity<MenuItem>()
                .HasOne(m => m.Category)
                .WithMany(c => c.MenuItems)
                .HasForeignKey(m => m.CategoryId);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Restaurant)
                .WithMany(r => r.Orders)
                .HasForeignKey(o => o.RestaurantId);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Status)
                .WithMany()
                .HasForeignKey(o => o.StatusId);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.MenuItem)
                .WithMany(mi => mi.OrderItems)
                .HasForeignKey(oi => oi.MenuItemId);

            modelBuilder.Entity<ActiveDelivery>()
                .HasOne(ad => ad.Order)
                .WithOne(o => o.ActiveDelivery)
                .HasForeignKey<ActiveDelivery>(ad => ad.OrderId);

            // Indexes for performance
            modelBuilder.Entity<MenuItem>()
                .HasIndex(m => m.RestaurantId);

            modelBuilder.Entity<MenuItem>()
                .HasIndex(m => m.CategoryId);

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.UserId);

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.StatusId);

            modelBuilder.Entity<CourierLocation>()
                .HasIndex(cl => new { cl.CourierId, cl.RecordedAt });

            // Decimal precision
            modelBuilder.Entity<MenuItem>()
                .Property(m => m.Price)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalPrice)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.DiscountAmount)
                .HasPrecision(10, 2);

            base.OnModelCreating(modelBuilder);
        }
    }