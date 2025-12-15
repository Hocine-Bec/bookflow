namespace Core.Entities;

public class BusinessHours : BaseEntity
{
    public Guid BusinessId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly? OpenTime { get; set; }
    public TimeOnly? CloseTime { get; set; }
    public bool IsOpen { get; set; } = true;
    public bool IsDeleted { get; set; } = false;

    public User Business { get; set; } = null!;
}
