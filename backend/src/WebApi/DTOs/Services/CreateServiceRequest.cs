using System.ComponentModel.DataAnnotations;

namespace WebApi.DTOs.Services;

public class CreateServiceRequest
{
    [Required(ErrorMessage = "Service name is required")]
    [StringLength(255, ErrorMessage = "Service name must not exceed 255 characters")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(1000, ErrorMessage = "Description must not exceed 1000 characters")]
    public string? Description { get; set; }
    
    [Required(ErrorMessage = "Duration is required")]
    [Range(15, 480, ErrorMessage = "Duration must be between 15 and 480 minutes (8 hours)")]
    public int DurationMinutes { get; set; }
    
    [Required(ErrorMessage = "Price is required")]
    [Range(0.01, 10000, ErrorMessage = "Price must be between 0.01 and 10000")]
    public decimal Price { get; set; }
}
