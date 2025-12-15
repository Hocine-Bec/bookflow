namespace Core.Interfaces;

/// <summary>
/// Structured logging interface for consistent logging across layers.
/// Avoids ILogger dependency in domain layer.
/// </summary>
public interface IAppLogger
{
    void LogInformation(string message, params object[] args);
    void LogWarning(string message, params object[] args);
    void LogError(string message, Exception? exception = null, params object[] args);
    void LogDebug(string message, params object[] args);
}
