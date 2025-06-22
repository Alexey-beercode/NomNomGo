using Microsoft.EntityFrameworkCore;
using NomNomGo.TrackingService.Domain.Entities;
using NomNomGo.TrackingService.Domain.Interfaces.Repositories;
using NomNomGo.TrackingService.Infrastructure.Persistence.Database;
using NomNomGo.TrackingService.Infrastructure.Repositories.Base;

namespace NomNomGo.TrackingService.Infrastructure.Repositories
{
    /// <summary>
    /// Реализация репозитория для активных доставок
    /// </summary>
    public class ActiveDeliveryRepository : BaseRepository<ActiveDelivery>, IActiveDeliveryRepository
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса ActiveDeliveryRepository
        /// </summary>
        /// <param name="dbContext">Контекст базы данных</param>
        public ActiveDeliveryRepository(ApplicationDbContext dbContext) : base(dbContext) { }

        /// <inheritdoc />
        public async Task<IReadOnlyList<ActiveDelivery>> GetByCourierIdAsync(
            Guid courierId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(ad => ad.CourierId == courierId)
                .OrderBy(ad => ad.EstimatedDeliveryTime)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<ActiveDelivery> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(ad => ad.OrderId == orderId, cancellationToken);
        }
    }
}