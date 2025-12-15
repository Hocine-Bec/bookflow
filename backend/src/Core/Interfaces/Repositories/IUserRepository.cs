using Core.Entities;

namespace Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for User entity data access operations.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Retrieves a user by their business slug (case-insensitive).
    /// Includes related services and business hours.
    /// </summary>
    /// <param name="slug">The business slug to search for</param>
    /// <returns>User entity with related data, or null if not found</returns>
    Task<User?> GetBySlugAsync(string slug);
    
    /// <summary>
    /// Checks if a user exists by ID.
    /// </summary>
    /// <param name="userId">The user ID to check</param>
    /// <returns>True if user exists, false otherwise</returns>
    Task<bool> ExistsAsync(Guid userId);
}
