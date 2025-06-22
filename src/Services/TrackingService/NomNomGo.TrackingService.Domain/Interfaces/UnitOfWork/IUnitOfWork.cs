using NomNomGo.TrackingService.Domain.Common;
using NomNomGo.TrackingService.Domain.Interfaces.Repositories;
using NomNomGo.TrackingService.Domain.Interfaces.Repositories.Base;

namespace NomNomGo.TrackingService.Domain.Interfaces.UnitOfWork
{
    /// <summary>
    /// Интерфейс Unit of Work для управления транзакциями и репозиториями
    /// </summary>
    public interface IUnitOfWork
    {
        /// <summary>
        /// Получает репозиторий для указанного типа сущности
        /// </summary>
        /// <typeparam name="TEntity">Тип сущности</typeparam>
        /// <returns>Репозиторий для типа сущности</returns>
        IRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity;
        
        /// <summary>
        /// Репозиторий местоположений курьеров
        /// </summary>
        ICourierLocationRepository CourierLocationRepository { get; }
        
        /// <summary>
        /// Репозиторий активных доставок
        /// </summary>
        IActiveDeliveryRepository ActiveDeliveryRepository { get; }
        
        /// <summary>
        /// Сохраняет все изменения, сделанные в контексте, в базу данных
        /// </summary>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Количество записей, записанных в базу данных</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Начинает новую транзакцию
        /// </summary>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Объект транзакции</returns>
        Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Фиксирует текущую транзакцию
        /// </summary>
        /// <param name="transaction">Транзакция для фиксации</param>
        /// <param name="cancellationToken">Токен отмены</param>
        Task CommitTransactionAsync(ITransaction transaction, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Откатывает текущую транзакцию
        /// </summary>
        /// <param name="transaction">Транзакция для отката</param>
        /// <param name="cancellationToken">Токен отмены</param>
        Task RollbackTransactionAsync(ITransaction transaction, CancellationToken cancellationToken = default);
    }
}