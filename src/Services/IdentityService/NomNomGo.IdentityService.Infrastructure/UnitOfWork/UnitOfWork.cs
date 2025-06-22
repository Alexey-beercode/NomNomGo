using CatalogueService.Domain.Interfaces.UnitOfWork;
using NomNomGo.IdentityService.Domain.Common;
using NomNomGo.IdentityService.Domain.Interfaces.Repositories;
using NomNomGo.IdentityService.Domain.Interfaces.Repositories.Base;
using NomNomGo.IdentityService.Domain.Interfaces.UnitOfWork;
using NomNomGo.IdentityService.Infrastructure.Persistence.Database;
using NomNomGo.IdentityService.Infrastructure.Repositories.Base;

namespace NomNomGo.IdentityService.Infrastructure.UnitOfWork;

  /// <summary>
    /// Implementation of the Unit of Work pattern for Entity Framework Core
    /// </summary>
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly ApplicationDbContext _dbContext;
        private Dictionary<Type, object> _repositories = new Dictionary<Type, object>();
        private bool _disposed;
        
        public IUserRepository UserRepository { get; }
        public IRoleRepository RoleRepository { get; }
        public IPermissionRepository PermissionRepository { get; }
        public IRefreshTokenRepository RefreshTokenRepository { get; }
        public IUserRoleRepository UserRoleRepository { get; }


        /// <summary>
        /// Инициализирует новый экземпляр класса UnitOfWork
        /// </summary>
        /// <param name="dbContext">Контекст базы данных</param>
        /// <param name="userRepository">Репозиторий пользователей</param>
        /// <param name="roleRepository">Репозиторий ролей</param>
        /// <param name="permissionRepository">Репозиторий разрешений</param>
        /// <param name="refreshTokenRepository">Репозиторий токенов обновления</param>
        /// <param name="userRoleRepository"></param>
        public UnitOfWork(
            ApplicationDbContext dbContext,
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IPermissionRepository permissionRepository,
            IRefreshTokenRepository refreshTokenRepository, IUserRoleRepository userRoleRepository)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            UserRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            RoleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
            PermissionRepository = permissionRepository ?? throw new ArgumentNullException(nameof(permissionRepository));
            RefreshTokenRepository = refreshTokenRepository ?? throw new ArgumentNullException(nameof(refreshTokenRepository));
            UserRoleRepository = userRoleRepository;
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
        public async Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            return new DbContextTransactionWrapper(transaction);
        }
        
        /// <inheritdoc />
        public async Task CommitTransactionAsync(ITransaction transaction, CancellationToken cancellationToken = default)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }
            
            await transaction.CommitAsync(cancellationToken);
        }
        
        /// <inheritdoc />
        public async Task RollbackTransactionAsync(ITransaction transaction, CancellationToken cancellationToken = default)
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