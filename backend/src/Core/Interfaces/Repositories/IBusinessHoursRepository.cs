using Core.Entities;

namespace Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for BusinessHours entity data access operations.
/// </summary>
public interface IBusinessHoursRepository
{
    /// <summary>
    /// Retrieves all business hours for a specific business.
    /// </summary>
    /// <param name="businessId">The business owner's ID</param>
    /// <returns>List of business hours for the business</returns>
    Task<List<BusinessHours>> GetByBusinessIdAsync(Guid businessId);
    
    /// <summary>
    /// Deletes multiple business hours records.
    /// </summary>
    /// <param name="hours">The business hours to delete</param>
    void DeleteRange(IEnumerable<BusinessHours> hours);
    
    /// <summary>
    /// Adds multiple business hours records.
    /// </summary>
    /// <param name="hours">The business hours to add</param>
    Task AddRangeAsync(IEnumerable<BusinessHours> hours);
    
    /// <summary>
    /// Persists all changes to the database.
    /// </summary>
    Task SaveChangesAsync();
}
