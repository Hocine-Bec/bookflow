using System.ComponentModel.DataAnnotations;

namespace WebApi.DTOs.BusinessHours;

public class SetBusinessHoursRequest
{
    [Required]
    [MinLength(7, ErrorMessage = "Must provide hours for all 7 days")]
    [MaxLength(7, ErrorMessage = "Must provide hours for exactly 7 days")]
    public List<BusinessHoursRequest> Hours { get; set; } = new();
}
