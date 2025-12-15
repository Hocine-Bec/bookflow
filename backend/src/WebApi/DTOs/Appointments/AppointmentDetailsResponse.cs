namespace WebApi.DTOs.Appointments;

public class AppointmentDetailsResponse
{
    public Guid Id { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string? BusinessPhone { get; set; }
    public DateOnly AppointmentDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public bool CanCancel { get; set; }
}
