using CatalogueService.Domain.Interfaces.UnitOfWork;
using NomNomGo.IdentityService.Domain.Common;
using NomNomGo.IdentityService.Domain.Interfaces.Repositories;
using NomNomGo.IdentityService.Domain.Interfaces.Repositories.Base;

namespace NomNomGo.IdentityService.Domain.Interfaces.UnitOfWork;

 /// <summary>
    /// Unit of Work interface for managing transactions and repositories
    /// </summary>
    public interface IUnitOfWork
    {
        /// <summary>
        /// Gets the repository for the specified entity type
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <returns>Repository for the entity type</returns>
        IRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity;
        
        /// <summary>
        /// Репозиторий пользователей
        /// </summary>
        IUserRepository UserRepository { get; }
        
        /// <summary>
        /// Репозиторий ролей
        /// </summary>
        IRoleRepository RoleRepository { get; }
        
        /// <summary>
        /// Репозиторий разрешений
        /// </summary>
        IPermissionRepository PermissionRepository { get; }
        
        /// <summary>
        /// Репозиторий токенов обновления
        /// </summary>
        IRefreshTokenRepository RefreshTokenRepository { get; }
        
        IUserRoleRepository UserRoleRepository { get; } 
        
        /// <summary>
        /// Saves all changes made in this context to the database
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of state entries written to the database</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Begins a new transaction
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Transaction object</returns>
        Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Commits the current transaction
        /// </summary>
        /// <param name="transaction">Transaction to commit</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task CommitTransactionAsync(ITransaction transaction, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Rolls back the current transaction
        /// </summary>
        /// <param name="transaction">Transaction to roll back</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task RollbackTransactionAsync(ITransaction transaction, CancellationToken cancellationToken = default);
    }