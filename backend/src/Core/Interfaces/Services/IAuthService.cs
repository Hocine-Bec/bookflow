using Core.Entities;

namespace Core.Interfaces.Services;

public interface IAuthService
{
    Task<(User? User, string ErrorMessage)> RegisterAsync(
        string email, 
        string password, 
        string businessName,
        string? businessPhone,
        string? businessDescription);
    
    Task<(User? User, string ErrorMessage)> LoginAsync(string email, string password);
    
    string GenerateJwtToken(User user);
    
    string HashPassword(string password);
    
    bool VerifyPassword(string password, string passwordHash);
}
