using NomNomGo.CouponService.Domain.Common;
using NomNomGo.CouponService.Domain.Interfaces.Repositories;
using NomNomGo.CouponService.Domain.Interfaces.Repositories.Base;
using NomNomGo.CouponService.Domain.Interfaces.UnitOfWork;
using NomNomGo.CouponService.Infrastructure.Persistence.Database;
using NomNomGo.CouponService.Infrastructure.Repositories.Base;

namespace NomNomGo.CouponService.Infrastructure.UnitOfWork;

public class CouponUnitOfWork : IUnitOfWork,IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Dictionary<Type, object> _repositories = new();
    private bool _disposed;

    public ICouponTypeRepository CouponTypeRepository { get; }
    public ICouponRepository CouponRepository { get; }
    public ICouponUsageRepository CouponUsageRepository { get; }

    public CouponUnitOfWork(
        ApplicationDbContext dbContext,
        ICouponTypeRepository couponTypeRepository,
        ICouponRepository couponRepository,
        ICouponUsageRepository couponUsageRepository)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        CouponTypeRepository = couponTypeRepository ?? throw new ArgumentNullException(nameof(couponTypeRepository));
        CouponRepository = couponRepository ?? throw new ArgumentNullException(nameof(couponRepository));
        CouponUsageRepository = couponUsageRepository ?? throw new ArgumentNullException(nameof(couponUsageRepository));
    }

    public IRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity
    {
        var type = typeof(TEntity);

        if (!_repositories.ContainsKey(type))
        {
            var repoType = typeof(BaseRepository<>).MakeGenericType(type);
            var instance = Activator.CreateInstance(repoType, _dbContext);
            _repositories[type] = instance!;
        }

        return (IRepository<TEntity>)_repositories[type];
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        return new DbContextTransactionWrapper(transaction);
    }

    public async Task CommitTransactionAsync(ITransaction transaction, CancellationToken cancellationToken = default)
    {
        if (transaction == null) throw new ArgumentNullException(nameof(transaction));
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task RollbackTransactionAsync(ITransaction transaction, CancellationToken cancellationToken = default)
    {
        if (transaction == null) throw new ArgumentNullException(nameof(transaction));
        await transaction.RollbackAsync(cancellationToken);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing) _dbContext.Dispose();
            _disposed = true;
        }
    }
}