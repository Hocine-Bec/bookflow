using Bogus;
using Core.Entities;

namespace Tests.Unit.Factories;

/// <summary>
/// Factory for generating fake BusinessHours entities for testing using Bogus.
/// </summary>
public static class BusinessHoursFactory
{
    private static readonly Faker _faker = new Faker();

    public static BusinessHours CreateFakeBusinessHours(
        Guid? id = null,
        Guid? businessId = null,
        DayOfWeek? dayOfWeek = null,
        TimeOnly? openTime = null,
        TimeOnly? closeTime = null,
        bool? isOpen = null)
    {
        var open = isOpen ?? _faker.Random.Bool(0.8f); // 80% chance of being open
        
        return new BusinessHours
        {
            Id = id ?? Guid.NewGuid(),
            BusinessId = businessId ?? Guid.NewGuid(),
            DayOfWeek = dayOfWeek ?? _faker.PickRandom<DayOfWeek>(),
            OpenTime = open ? (openTime ?? new TimeOnly(9, 0)) : null,
            CloseTime = open ? (closeTime ?? new TimeOnly(17, 0)) : null,
            IsOpen = open
        };
    }

    public static List<BusinessHours> CreateFakeBusinessHoursForWeek(
        Guid? businessId = null,
        bool allOpen = true)
    {
        var hours = new List<BusinessHours>();
        var busId = businessId ?? Guid.NewGuid();
        
        for (int i = 0; i < 7; i++)
        {
            var dayOfWeek = (DayOfWeek)i;
            var isOpen = allOpen || (i >= 1 && i <= 5); // Mon-Fri if not all open
            
            hours.Add(new BusinessHours
            {
                Id = Guid.NewGuid(),
                BusinessId = busId,
                DayOfWeek = dayOfWeek,
                OpenTime = isOpen ? new TimeOnly(9, 0) : null,
                CloseTime = isOpen ? new TimeOnly(17, 0) : null,
                IsOpen = isOpen
            });
        }
        
        return hours;
    }

    public static List<BusinessHours> CreateFakeBusinessHoursWithCustomTimes(
        Guid businessId,
        Dictionary<DayOfWeek, (TimeOnly open, TimeOnly close)> schedule)
    {
        var hours = new List<BusinessHours>();
        
        for (int i = 0; i < 7; i++)
        {
            var dayOfWeek = (DayOfWeek)i;
            var hasSchedule = schedule.ContainsKey(dayOfWeek);
            
            hours.Add(new BusinessHours
            {
                Id = Guid.NewGuid(),
                BusinessId = businessId,
                DayOfWeek = dayOfWeek,
                OpenTime = hasSchedule ? schedule[dayOfWeek].open : null,
                CloseTime = hasSchedule ? schedule[dayOfWeek].close : null,
                IsOpen = hasSchedule
            });
        }
        
        return hours;
    }
}
