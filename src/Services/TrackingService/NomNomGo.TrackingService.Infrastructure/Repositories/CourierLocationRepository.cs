using Microsoft.EntityFrameworkCore;
using NomNomGo.TrackingService.Domain.Entities;
using NomNomGo.TrackingService.Domain.Interfaces.Repositories;
using NomNomGo.TrackingService.Infrastructure.Persistence.Database;
using NomNomGo.TrackingService.Infrastructure.Repositories.Base;

namespace NomNomGo.TrackingService.Infrastructure.Repositories
{
    /// <summary>
    /// Реализация репозитория для местоположений курьеров
    /// </summary>
    public class CourierLocationRepository : BaseRepository<CourierLocation>, ICourierLocationRepository
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса CourierLocationRepository
        /// </summary>
        /// <param name="dbContext">Контекст базы данных</param>
        public CourierLocationRepository(ApplicationDbContext dbContext) : base(dbContext) { }

        /// <inheritdoc />
        public async Task<IReadOnlyList<CourierLocation>> GetCourierLocationHistoryAsync(
            Guid courierId,
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(cl => cl.CourierId == courierId && 
                             cl.RecordedAt >= startDate && 
                             cl.RecordedAt <= endDate)
                .OrderBy(cl => cl.RecordedAt)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<CourierLocation> GetLastLocationAsync(Guid courierId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(cl => cl.CourierId == courierId)
                .OrderByDescending(cl => cl.RecordedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}