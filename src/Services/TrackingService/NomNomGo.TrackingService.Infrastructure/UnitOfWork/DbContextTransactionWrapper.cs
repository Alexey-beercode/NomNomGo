using CatalogueService.Domain.Interfaces.UnitOfWork;
using Microsoft.EntityFrameworkCore.Storage;
using ITransaction = NomNomGo.TrackingService.Domain.Interfaces.UnitOfWork.ITransaction;

namespace NomNomGo.TrackingService.Infrastructure.UnitOfWork;

/// <summary>
/// Wrapper for Entity Framework Core DbContextTransaction
/// </summary>
public class DbContextTransactionWrapper : ITransaction
{
    private readonly IDbContextTransaction _transaction;

    /// <summary>
    /// Initializes a new instance of the DbContextTransactionWrapper class
    /// </summary>
    /// <param name="transaction">Database context transaction</param>
    public DbContextTransactionWrapper(IDbContextTransaction transaction)
    {
        _transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
    }
        
    /// <inheritdoc />
    public void Commit()
    {
        _transaction.Commit();
    }
        
    /// <inheritdoc />
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await _transaction.CommitAsync(cancellationToken);
    }
        
    /// <inheritdoc />
    public void Rollback()
    {
        _transaction.Rollback();
    }
        
    /// <inheritdoc />
    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        await _transaction.RollbackAsync(cancellationToken);
    }
        
    /// <inheritdoc />
    public void Dispose()
    {
        _transaction.Dispose();
    }
        
    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await _transaction.DisposeAsync();
    }
}