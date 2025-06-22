using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NomNomGo.IdentityService.Domain.Entities;
using NomNomGo.IdentityService.Domain.Entities.Relationships;

namespace NomNomGo.IdentityService.Infrastructure.Persistence.Database
{
    public class ApplicationDbContext : DbContext
    {
        private IDbContextTransaction? _currentTransaction;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet для каждой сущности
        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

        // Транзакции
        public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction != null)
            {
                return _currentTransaction;
            }

            _currentTransaction = await Database.BeginTransactionAsync(cancellationToken);
            return _currentTransaction;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Применение всех конфигураций из текущей сборки
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // Добавляем сидер данных
            SeedData(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            // Создаем роли
            var userRoleId = Guid.NewGuid();
            var adminRoleId = Guid.NewGuid();
            
            var roles = new[]
            {
                new Role
                {
                    Id = userRoleId,
                    Name = "User",
                },
                new Role
                {
                    Id = adminRoleId,
                    Name = "Admin",
                },
                new Role
                {
                Id = Guid.NewGuid(),
                Name = "Courier",
                }
            };

            modelBuilder.Entity<Role>().HasData(roles);

            // Создаем админа
            var adminUserId = Guid.NewGuid();
            var adminUser = new User
            {
                Id = adminUserId,
                Email = "admin@nomnom.local",
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin1232"), // Хешируем пароль
                PhoneNumber = "+375447777777",
                IsBlocked = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            modelBuilder.Entity<User>().HasData(adminUser);

            // Назначаем роль админа
            var userRole = new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = adminUserId,
                RoleId = adminRoleId
            };

            modelBuilder.Entity<UserRole>().HasData(userRole);

            // Создаем пермишены (если нужно)
            var permissions = new[]
            {
                new Permission
                {
                    Id = Guid.NewGuid(),
                    Name = "ManageUsers",
                },
                new Permission
                {
                    Id = Guid.NewGuid(),
                    Name = "ManageRestaurants", 
                },
                new Permission
                {
                    Id = Guid.NewGuid(),
                    Name = "ManageOrders",
                },
                new Permission
                {
                    Id = Guid.NewGuid(),
                    Name = "ViewReports",
                }
            };

            modelBuilder.Entity<Permission>().HasData(permissions);

            // Назначаем все пермишены роли Admin
            var rolePermissions = permissions.Select(p => new RolePermission
            {
                RoleId = adminRoleId,
                PermissionId = p.Id
            }).ToArray();

            modelBuilder.Entity<RolePermission>().HasData(rolePermissions);
        }
    }
}