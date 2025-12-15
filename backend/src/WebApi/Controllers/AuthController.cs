using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApi.DTOs.Auth;
using WebApi.DTOs.Common;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IConfiguration _configuration;
    
    public AuthController(IAuthService authService, IConfiguration configuration)
    {
        _authService = authService;
        _configuration = configuration;
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );
            
            return BadRequest(new ErrorResponse("Validation failed", errors));
        }
        
        var (user, errorMessage) = await _authService.RegisterAsync(
            request.Email,
            request.Password,
            request.BusinessName,
            request.BusinessPhone,
            request.BusinessDescription
        );
        
        if (user == null)
        {
            return BadRequest(new ErrorResponse(errorMessage));
        }
        
        var token = _authService.GenerateJwtToken(user);
        var baseUrl = Environment.GetEnvironmentVariable("APP_BASE_URL") ?? "http://localhost:5173";
        
        var response = new AuthResponse
        {
            UserId = user.Id,
            Email = user.Email,
            BusinessName = user.BusinessName,
            BusinessSlug = user.BusinessSlug,
            BookingUrl = $"{baseUrl}/{user.BusinessSlug}",
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(int.Parse(
        	Environment.GetEnvironmentVariable("JWT_EXPIRY_MINUTES") ?? "15"))
        };
        
        return CreatedAtAction(nameof(GetCurrentUser), response);
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );
            
            return BadRequest(new ErrorResponse("Validation failed", errors));
        }
        
        var (user, errorMessage) = await _authService.LoginAsync(request.Email, request.Password);
        
        if (user == null)
        {
            return Unauthorized(new ErrorResponse(errorMessage));
        }
        
        var token = _authService.GenerateJwtToken(user);
        var baseUrl = Environment.GetEnvironmentVariable("APP_BASE_URL") ?? "http://localhost:5173";
        
        var response = new AuthResponse
        {
            UserId = user.Id,
            Email = user.Email,
            BusinessName = user.BusinessName,
            BusinessSlug = user.BusinessSlug,
            BookingUrl = $"{baseUrl}/{user.BusinessSlug}",
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(int.Parse(
        	Environment.GetEnvironmentVariable("JWT_EXPIRY_MINUTES") ?? "24"))
        };
        
        return Ok(response);
    }
    
    [HttpGet("me")]
    [Authorize]
    public IActionResult GetCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var businessSlug = User.FindFirst("BusinessSlug")?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new ErrorResponse("Invalid token"));
        }
        
        return Ok(new
        {
            userId = Guid.Parse(userId),
            email,
            businessSlug
        });
    }
}
