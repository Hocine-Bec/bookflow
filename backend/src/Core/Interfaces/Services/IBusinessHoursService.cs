namespace Core.Interfaces.Services;

/// <summary>
/// Service interface for managing business operating hours.
/// Handles retrieval and configuration of business hours for all days of the week.
/// </summary>
public interface IBusinessHoursService
{
    /// <summary>
    /// Retrieves business hours for a specific business.
    /// </summary>
    /// <param name="businessId">The business owner's ID</param>
    /// <returns>List of business hours for all configured days</returns>
    Task<List<dynamic>> GetBusinessHoursByBusinessIdAsync(Guid businessId);
    
    /// <summary>
    /// Sets or updates business hours for all days of the week.
    /// Validates that all 7 days are provided and time ranges are valid.
    /// </summary>
    /// <param name="businessId">The business owner's ID</param>
    /// <param name="request">Business hours configuration request (DTO)</param>
    /// <returns>Updated business hours for all days</returns>
    Task<List<dynamic>> SetBusinessHoursAsync(Guid businessId, dynamic request);
}
