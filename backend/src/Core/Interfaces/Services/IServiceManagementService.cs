namespace Core.Interfaces.Services;

/// <summary>
/// Service interface for managing business services (CRUD operations).
/// Handles creation, retrieval, updating, and deletion of services.
/// </summary>
public interface IServiceManagementService
{
    /// <summary>
    /// Retrieves a single active service by ID.
    /// </summary>
    /// <param name="serviceId">The service ID</param>
    /// <returns>Service details, or null if not found or inactive</returns>
    Task<dynamic?> GetServiceByIdAsync(Guid serviceId);
    
    /// <summary>
    /// Retrieves all services for a specific business.
    /// </summary>
    /// <param name="businessId">The business owner's ID</param>
    /// <returns>List of services owned by the business</returns>
    Task<List<dynamic>> GetServicesByBusinessIdAsync(Guid businessId);
    
    /// <summary>
    /// Creates a new service for a business.
    /// </summary>
    /// <param name="businessId">The business owner's ID</param>
    /// <param name="request">Service creation request (DTO)</param>
    /// <returns>Created service details</returns>
    Task<dynamic> CreateServiceAsync(Guid businessId, dynamic request);
    
    /// <summary>
    /// Updates an existing service.
    /// Validates that the service belongs to the specified business.
    /// </summary>
    /// <param name="businessId">The business owner's ID</param>
    /// <param name="serviceId">The service ID to update</param>
    /// <param name="request">Service update request (DTO)</param>
    /// <returns>Updated service details, or null if not found or unauthorized</returns>
    Task<dynamic?> UpdateServiceAsync(Guid businessId, Guid serviceId, dynamic request);
    
    /// <summary>
    /// Soft deletes a service (marks as inactive).
    /// Validates that the service belongs to the specified business.
    /// </summary>
    /// <param name="businessId">The business owner's ID</param>
    /// <param name="serviceId">The service ID to delete</param>
    /// <returns>True if deleted successfully, false if not found or unauthorized</returns>
    Task<bool> DeleteServiceAsync(Guid businessId, Guid serviceId);
}
