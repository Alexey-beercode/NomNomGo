using CatalogueService.Domain.Interfaces.UnitOfWork;
using NomNomGo.IdentityService.Domain.Interfaces.UnitOfWork;
using NomNomGo.TrackingService.Domain.Common;
using NomNomGo.TrackingService.Domain.Interfaces.Repositories;
using NomNomGo.TrackingService.Domain.Interfaces.Repositories.Base;
using NomNomGo.TrackingService.Infrastructure.Persistence.Database;
using NomNomGo.TrackingService.Infrastructure.Repositories.Base;
using BaseEntity = NomNomGo.TrackingService.Domain.Common.BaseEntity;
using IUnitOfWork = NomNomGo.TrackingService.Domain.Interfaces.UnitOfWork.IUnitOfWork;

namespace NomNomGo.TrackingService.Infrastructure.UnitOfWork
{
    /// <summary>
    /// Реализация паттерна Unit of Work для Entity Framework Core
    /// </summary>
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly ApplicationDbContext _dbContext;
        private Dictionary<Type, object> _repositories = new Dictionary<Type, object>();
        private bool _disposed;

        public ICourierLocationRepository CourierLocationRepository { get; }
        public IActiveDeliveryRepository ActiveDeliveryRepository { get; }

        /// <summary>
        /// Инициализирует новый экземпляр класса UnitOfWork
        /// </summary>
        /// <param name="dbContext">Контекст базы данных</param>
        /// <param name="courierLocationRepository">Репозиторий местоположений курьеров</param>
        /// <param name="activeDeliveryRepository">Репозиторий активных доставок</param>
        public UnitOfWork(
            ApplicationDbContext dbContext,
            ICourierLocationRepository courierLocationRepository,
            IActiveDeliveryRepository activeDeliveryRepository)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            CourierLocationRepository = courierLocationRepository ??
                                        throw new ArgumentNullException(nameof(courierLocationRepository));
            ActiveDeliveryRepository = activeDeliveryRepository ??
                                       throw new ArgumentNullException(nameof(activeDeliveryRepository));
        }

        /// <inheritdoc />
        public IRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity
        {
            var type = typeof(TEntity);

            if (!_repositories.ContainsKey(type))
            {
                var repositoryType = typeof(BaseRepository<>).MakeGenericType(type);
                var repository = Activator.CreateInstance(repositoryType, _dbContext);

                _repositories.Add(type, repository);
            }

            return (IRepository<TEntity>)_repositories[type];
        }

        /// <inheritdoc />
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Domain.Interfaces.UnitOfWork.ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            return new DbContextTransactionWrapper(transaction);
        }

        public Task CommitTransactionAsync(Domain.Interfaces.UnitOfWork.ITransaction transaction, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task RollbackTransactionAsync(Domain.Interfaces.UnitOfWork.ITransaction transaction, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task CommitTransactionAsync(ITransaction transaction,
            CancellationToken cancellationToken = default)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            await transaction.CommitAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task RollbackTransactionAsync(ITransaction transaction,
            CancellationToken cancellationToken = default)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            await transaction.RollbackAsync(cancellationToken);
        }

        /// <summary>
        /// Disposes the context and repositories
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the context and repositories
        /// </summary>
        /// <param name="disposing">Whether the method is called from Dispose()</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _dbContext.Dispose();
                }

                _disposed = true;
            }
        }
    }
}