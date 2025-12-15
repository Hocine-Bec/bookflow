using Core.Entities;

namespace Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for Service entity data access operations.
/// </summary>
public interface IServiceRepository
{
    /// <summary>
    /// Retrieves a service by ID.
    /// </summary>
    /// <param name="id">The service ID</param>
    /// <returns>Service entity, or null if not found</returns>
    Task<Service?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Retrieves an active service by ID.
    /// </summary>
    /// <param name="id">The service ID</param>
    /// <returns>Active service entity, or null if not found or inactive</returns>
    Task<Service?> GetActiveByIdAsync(Guid id);
    
    /// <summary>
    /// Retrieves all services for a specific business.
    /// </summary>
    /// <param name="businessId">The business owner's ID</param>
    /// <returns>List of services owned by the business</returns>
    Task<List<Service>> GetByBusinessIdAsync(Guid businessId);
    
    /// <summary>
    /// Adds a new service to the repository.
    /// </summary>
    /// <param name="service">The service entity to add</param>
    Task AddAsync(Service service);
    
    /// <summary>
    /// Updates an existing service.
    /// </summary>
    /// <param name="service">The service entity to update</param>
    void Update(Service service);
    
    /// <summary>
    /// Persists all changes to the database.
    /// </summary>
    Task SaveChangesAsync();
}
