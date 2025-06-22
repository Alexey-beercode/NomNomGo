using NomNomGo.TrackingService.Domain.Entities;
using NomNomGo.TrackingService.Domain.Interfaces.Repositories.Base;

namespace NomNomGo.TrackingService.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Интерфейс репозитория для работы с активными доставками
    /// </summary>
    public interface IActiveDeliveryRepository : IRepository<ActiveDelivery>
    {
        /// <summary>
        /// Получает все активные доставки для указанного курьера
        /// </summary>
        /// <param name="courierId">Идентификатор курьера</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Коллекция активных доставок</returns>
        Task<IReadOnlyList<ActiveDelivery>> GetByCourierIdAsync(
            Guid courierId,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Получает активную доставку по идентификатору заказа
        /// </summary>
        /// <param name="orderId">Идентификатор заказа</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Активная доставка или null, если не найдена</returns>
        Task<ActiveDelivery> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    }
}