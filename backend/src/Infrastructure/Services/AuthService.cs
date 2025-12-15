using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Core.Entities;
using Core.Enums;
using Core.Interfaces.Services;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;

namespace Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly ISlugService _slugService;
    private readonly IConfiguration _configuration;
    
    public AuthService(
        AppDbContext context, 
        ISlugService slugService,
        IConfiguration configuration)
    {
        _context = context;
        _slugService = slugService;
        _configuration = configuration;
    }
    
    public async Task<(User? User, string ErrorMessage)> RegisterAsync(
        string email,
        string password,
        string businessName,
        string? businessPhone,
        string? businessDescription)
    {
        // Check if email already exists
        if (await _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower()))
        {
            return (null, "Email is already registered");
        }
        
        // Generate unique slug
        var slug = await _slugService.GenerateUniqueSlugAsync(businessName);
        
        // Create user
        var user = new User
        {
            Email = email.ToLower(),
            PasswordHash = HashPassword(password),
            Role = UserRole.Business,
            BusinessName = businessName,
            BusinessSlug = slug,
            BusinessPhone = businessPhone,
            BusinessDescription = businessDescription,
            IsEmailVerified = false
        };
        
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        
        return (user, string.Empty);
    }
    
    public async Task<(User? User, string ErrorMessage)> LoginAsync(string email, string password)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        
        if (user == null)
        {
            return (null, "Invalid email or password");
        }
        
        if (!VerifyPassword(password, user.PasswordHash))
        {
            return (null, "Invalid email or password");
        }
        
        return (user, string.Empty);
    }
    
    public string GenerateJwtToken(User user)
{
    var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
        ?? throw new InvalidOperationException("JWT_SECRET_KEY is not set");

    var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "BookingSystem.API";
    var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "BookingSystem.Client";
    var expiryMinutes = int.Parse(
        Environment.GetEnvironmentVariable("JWT_EXPIRY_MINUTES") ?? "15"
    );

    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role.ToString()),
        new Claim("BusinessSlug", user.BusinessSlug),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

    var token = new JwtSecurityToken(
        issuer: issuer,
        audience: audience,
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}

    
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
    
    public bool VerifyPassword(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}
