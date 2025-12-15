using Core.Entities;

namespace Core.Interfaces.Repositories;

/// <summary>
/// Generic repository interface for data access operations.
/// Provides CRUD operations and query support with multi-tenant isolation.
/// </summary>
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> DeleteAsync(T entity);
    IQueryable<T> Query();
}

/// <summary>
/// Unit of Work pattern to manage multiple repositories and transactions.
/// Ensures consistency across multiple data access operations.
/// </summary>
public interface IUnitOfWork : IAsyncDisposable
{
    IRepository<T> Repository<T>() where T : BaseEntity;
    Task<int> SaveChangesAsync();
    Task<bool> BeginTransactionAsync();
    Task<bool> CommitTransactionAsync();
    Task<bool> RollbackTransactionAsync();
}
