namespace NomNomGo.TrackingService.Domain.Interfaces.UnitOfWork;

/// <summary>
/// Database context transaction interface
/// </summary>
public interface ITransaction : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Commits the transaction
    /// </summary>
    void Commit();
        
    /// <summary>
    /// Commits the transaction asynchronously
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task CommitAsync(CancellationToken cancellationToken = default);
        
    /// <summary>
    /// Rolls back the transaction
    /// </summary>
    void Rollback();
        
    /// <summary>
    /// Rolls back the transaction asynchronously
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}