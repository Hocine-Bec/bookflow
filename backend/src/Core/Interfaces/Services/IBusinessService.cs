namespace Core.Interfaces.Services;

/// <summary>
/// Service interface for business profile operations.
/// Handles retrieving business information and hours.
/// </summary>
public interface IBusinessService
{
    /// <summary>
    /// Retrieves a business profile by its unique slug, including active services.
    /// </summary>
    /// <param name="slug">The business slug (case-insensitive)</param>
    /// <returns>Business profile with services, or null if not found</returns>
    Task<dynamic?> GetBusinessBySlugAsync(string slug);
    
    /// <summary>
    /// Retrieves business hours for a specific business.
    /// </summary>
    /// <param name="businessId">The business ID</param>
    /// <returns>List of business hours for all days of the week</returns>
    Task<dynamic> GetBusinessHoursAsync(Guid businessId);
}
