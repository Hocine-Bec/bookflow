namespace WebApi.DTOs.BusinessHours;

public class BusinessHoursResponse
{
    public Guid Id { get; set; }
    public int DayOfWeek { get; set; }
    public string DayName { get; set; } = string.Empty;
    public TimeOnly? OpenTime { get; set; }
    public TimeOnly? CloseTime { get; set; }
    public bool IsOpen { get; set; }
}
