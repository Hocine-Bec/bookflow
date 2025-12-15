using Core.Enums;

namespace Core.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public string BusinessSlug { get; set; } = string.Empty;
    public string? BusinessPhone { get; set; }
    public string? BusinessDescription { get; set; }
    public bool IsEmailVerified { get; set; }

    public ICollection<Service> Services { get; set; } = new List<Service>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<BusinessHours> BusinessHours { get; set; } = new List<BusinessHours>();
}
