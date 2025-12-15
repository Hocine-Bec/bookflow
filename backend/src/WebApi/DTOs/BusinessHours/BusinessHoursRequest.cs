using System.ComponentModel.DataAnnotations;

namespace WebApi.DTOs.BusinessHours;

public class BusinessHoursRequest
{
    [Required]
    [Range(0, 6, ErrorMessage = "DayOfWeek must be between 0 (Sunday) and 6 (Saturday)")]
    public int DayOfWeek { get; set; }
    
    public TimeOnly? OpenTime { get; set; }
    
    public TimeOnly? CloseTime { get; set; }
    
    [Required]
    public bool IsOpen { get; set; }
}
