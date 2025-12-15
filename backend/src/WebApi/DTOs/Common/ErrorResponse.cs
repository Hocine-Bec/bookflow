namespace WebApi.DTOs.Common;

public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, string[]>? Errors { get; set; }
    
    public ErrorResponse(string message)
    {
        Message = message;
    }
    
    public ErrorResponse(string message, Dictionary<string, string[]> errors)
    {
        Message = message;
        Errors = errors;
    }
}
