namespace Core.Common;

/// <summary>
/// Represents the result of an operation with either success data or failure information.
/// Replaces null returns and exceptions for expected errors.
/// </summary>
public abstract record Result
{
    public sealed record Success(object? Data = null) : Result;
    public sealed record Failure(string Code, string Message, Dictionary<string, string[]>? Errors = null) : Result;
    
    public TResult Match<TResult>(
        Func<object?, TResult> onSuccess,
        Func<string, string, Dictionary<string, string[]>?, TResult> onFailure) =>
        this switch
        {
            Success s => onSuccess(s.Data),
            Failure f => onFailure(f.Code, f.Message, f.Errors),
            _ => throw new InvalidOperationException("Unknown result type")
        };

    public void Match(
        Action<object?> onSuccess,
        Action<string, string, Dictionary<string, string[]>?> onFailure)
    {
        switch (this)
        {
            case Success s:
                onSuccess(s.Data);
                break;
            case Failure f:
                onFailure(f.Code, f.Message, f.Errors);
                break;
        }
    }
}

/// <summary>
/// Generic result that preserves type information for success case.
/// </summary>
public abstract record Result<T> : Result
{
    public sealed record Success(T Data) : Result<T>;
    public sealed record Failure(string Code, string Message, Dictionary<string, string[]>? Errors = null) : Result<T>;

    public TResult Match<TResult>(
        Func<T, TResult> onSuccess,
        Func<string, string, Dictionary<string, string[]>?, TResult> onFailure) =>
        this switch
        {
            Success s => onSuccess(s.Data),
            Failure f => onFailure(f.Code, f.Message, f.Errors),
            _ => throw new InvalidOperationException("Unknown result type")
        };

    public void Match(
        Action<T> onSuccess,
        Action<string, string, Dictionary<string, string[]>?> onFailure)
    {
        switch (this)
        {
            case Success s:
                onSuccess(s.Data);
                break;
            case Failure f:
                onFailure(f.Code, f.Message, f.Errors);
                break;
        }
    }

    public static implicit operator Result<T>(T data) => new Success(data);
}

/// <summary>
/// Standard error codes used across the application.
/// </summary>
public static class ErrorCodes
{
    // Validation errors
    public const string ValidationFailed = "VALIDATION_FAILED";
    public const string InvalidInput = "INVALID_INPUT";
    
    // Not found errors
    public const string NotFound = "NOT_FOUND";
    public const string ResourceNotFound = "RESOURCE_NOT_FOUND";
    
    // Authorization errors
    public const string Unauthorized = "UNAUTHORIZED";
    public const string Forbidden = "FORBIDDEN";
    
    // Business logic errors
    public const string ConflictError = "CONFLICT_ERROR";
    public const string OperationFailed = "OPERATION_FAILED";
    
    // System errors
    public const string InternalError = "INTERNAL_ERROR";
    public const string DatabaseError = "DATABASE_ERROR";
}
