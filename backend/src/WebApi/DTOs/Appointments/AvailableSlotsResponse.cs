namespace WebApi.DTOs.Appointments;

public class AvailableSlotsResponse
{
    public Guid ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public DateOnly Date { get; set; }
    public List<AvailableSlotResponse> AvailableSlots { get; set; } = new();
}
