using System.ComponentModel.DataAnnotations;

namespace WebApi.DTOs.Appointments;

public class CreateAppointmentRequest
{
    [Required(ErrorMessage = "Service ID is required")]
    public Guid ServiceId { get; set; }
    
    [Required(ErrorMessage = "Appointment date is required")]
    public DateOnly AppointmentDate { get; set; }
    
    [Required(ErrorMessage = "Start time is required")]
    public TimeOnly StartTime { get; set; }
    
    [Required(ErrorMessage = "Customer name is required")]
    [StringLength(255, ErrorMessage = "Name must not exceed 255 characters")]
    public string CustomerName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Customer email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string CustomerEmail { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Customer phone is required")]
    [Phone(ErrorMessage = "Invalid phone number")]
    public string CustomerPhone { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "Notes must not exceed 500 characters")]
    public string? Notes { get; set; }
}
