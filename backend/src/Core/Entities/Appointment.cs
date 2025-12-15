using Core.Enums;

namespace Core.Entities;

public class Appointment : BaseEntity
{
    public Guid ServiceId { get; set; }
    public Guid BusinessId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public DateOnly AppointmentDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
    public string? Notes { get; set; }
    public string CancellationToken { get; set; } = string.Empty;
    public string ConfirmationNumber { get; set; } = string.Empty;
    public bool IsDeleted { get; set; } = false;

    public Service Service { get; set; } = null!;
    public User Business { get; set; } = null!;
}
