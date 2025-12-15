namespace Core.Interfaces;

/// <summary>
/// Contract for request validation using FluentValidation.
/// All request DTOs should implement this marker interface.
/// </summary>
public interface IValidatedRequest
{
}

/// <summary>
/// Contract for service-level validation beyond request DTOs.
/// Used for complex business logic validation.
/// </summary>
public interface IBusinessValidator<T>
{
    Task<Dictionary<string, string[]>> ValidateAsync(T entity);
}
