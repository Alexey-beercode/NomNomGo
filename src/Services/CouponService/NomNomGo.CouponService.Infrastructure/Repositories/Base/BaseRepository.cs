using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using NomNomGo.CouponService.Domain.Common;
using NomNomGo.CouponService.Domain.Interfaces.Repositories.Base;
using NomNomGo.CouponService.Infrastructure.Persistence.Database;

namespace NomNomGo.CouponService.Infrastructure.Repositories.Base
{
    /// <summary>
    /// Generic repository implementation for Entity Framework Core
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public class BaseRepository<T> : IRepository<T> where T : BaseEntity
    {
        protected readonly ApplicationDbContext _dbContext;
        protected readonly DbSet<T> _dbSet;
        
        /// <summary>
        /// Initializes a new instance of the Repository class
        /// </summary>
        /// <param name="dbContext">Database context</param>
        public BaseRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _dbSet = _dbContext.Set<T>();
        }
        
        /// <inheritdoc />
        public virtual async Task<T> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FindAsync([id], cancellationToken);
        }
        
        /// <inheritdoc />
        public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet.AsNoTracking().ToListAsync(cancellationToken);
        }
        
        /// <inheritdoc />
        public virtual async Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(predicate)
                .ToListAsync(cancellationToken);
        }
        
        /// <inheritdoc />
        public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(entity);
            
            await _dbSet.AddAsync(entity, cancellationToken);
            
            return entity;
        }
        
        /// <inheritdoc />
        public virtual void Update(T entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            
            _dbContext.Entry(entity).State = EntityState.Modified;
        }
        
        /// <inheritdoc />
        public virtual void Delete(T entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            _dbSet.Remove(entity);
        }
    }
}