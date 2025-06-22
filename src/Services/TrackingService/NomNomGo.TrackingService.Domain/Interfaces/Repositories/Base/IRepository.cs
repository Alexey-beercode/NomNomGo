using System.Linq.Expressions;
using BaseEntity = NomNomGo.TrackingService.Domain.Common.BaseEntity;

namespace NomNomGo.TrackingService.Domain.Interfaces.Repositories.Base
{
    /// <summary>
    /// Generic repository interface for basic CRUD operations
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public interface IRepository<T> where T : BaseEntity
    {
        /// <summary>
        /// Gets an entity by its identifier
        /// </summary>
        /// <param name="id">Entity identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Entity or null if not found</returns>
        Task<T> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets all entities
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Collection of entities</returns>
        Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets entities matching the specified predicate
        /// </summary>
        /// <param name="predicate">Filter expression</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Collection of filtered entities</returns>
        Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Adds a new entity to the database context
        /// </summary>
        /// <param name="entity">Entity to add</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Added entity</returns>
        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Updates an existing entity in the database context
        /// </summary>
        /// <param name="entity">Entity to update</param>
        void Update(T entity);
        
        /// <summary>
        /// Deletes an entity from the database context
        /// </summary>
        /// <param name="entity">Entity to delete</param>
        void Delete(T entity);
    }
}