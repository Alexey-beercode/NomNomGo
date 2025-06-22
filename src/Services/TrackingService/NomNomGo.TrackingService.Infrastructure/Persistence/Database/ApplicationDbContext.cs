using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NomNomGo.IdentityService.Domain.Entities;
using NomNomGo.IdentityService.Domain.Entities.Relationships;
using NomNomGo.TrackingService.Domain.Entities;

namespace NomNomGo.TrackingService.Infrastructure.Persistence.Database
{
    public class ApplicationDbContext : DbContext
    {
        private IDbContextTransaction? _currentTransaction;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet для каждой сущности
        public DbSet<ActiveDelivery> ActiveDeliveries { get; set; }
        public DbSet<CourierLocation> CourierLocations { get; set; }

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

            base.OnModelCreating(modelBuilder);
        }
        
    }
}