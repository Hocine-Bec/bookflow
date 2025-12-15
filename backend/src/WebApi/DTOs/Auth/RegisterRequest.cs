using System.ComponentModel.DataAnnotations;

namespace WebApi.DTOs.Auth;

public class RegisterRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    public string Password { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Business name is required")]
    [StringLength(255, ErrorMessage = "Business name must not exceed 255 characters")]
    public string BusinessName { get; set; } = string.Empty;
    
    [Phone(ErrorMessage = "Invalid phone number")]
    public string? BusinessPhone { get; set; }
    
    [StringLength(1000, ErrorMessage = "Description must not exceed 1000 characters")]
    public string? BusinessDescription { get; set; }
}
