using NomNomGo.TrackingService.Domain.Entities;
using NomNomGo.TrackingService.Domain.Interfaces.Repositories.Base;

namespace NomNomGo.TrackingService.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Интерфейс репозитория для работы с местоположениями курьеров
    /// </summary>
    public interface ICourierLocationRepository : IRepository<CourierLocation>
    {
        /// <summary>
        /// Получает историю местоположений курьера за указанный период
        /// </summary>
        /// <param name="courierId">Идентификатор курьера</param>
        /// <param name="startDate">Начальная дата</param>
        /// <param name="endDate">Конечная дата</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Коллекция местоположений курьера</returns>
        Task<IReadOnlyList<CourierLocation>> GetCourierLocationHistoryAsync(
            Guid courierId,
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Получает последнее местоположение курьера
        /// </summary>
        /// <param name="courierId">Идентификатор курьера</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Последнее местоположение курьера или null</returns>
        Task<CourierLocation> GetLastLocationAsync(Guid courierId, CancellationToken cancellationToken = default);
    }
}