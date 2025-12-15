using Core.Entities;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;

namespace Core.Services;

/// <summary>
/// Business service for managing business hours operations.
/// Contains pure business logic with validation rules and no direct database dependencies.
/// </summary>
public class BusinessHoursService : IBusinessHoursService
{
    private readonly IBusinessHoursRepository _businessHoursRepository;

    public BusinessHoursService(IBusinessHoursRepository businessHoursRepository)
    {
        _businessHoursRepository = businessHoursRepository;
    }

    public async Task<List<dynamic>> GetBusinessHoursByBusinessIdAsync(Guid businessId)
    {
        var hours = await _businessHoursRepository.GetByBusinessIdAsync(businessId);

        // Map to DTOs
        return hours.Select(h => new
        {
            Id = h.Id,
            DayOfWeek = (int)h.DayOfWeek,
            DayName = h.DayOfWeek.ToString(),
            OpenTime = h.OpenTime,
            CloseTime = h.CloseTime,
            IsOpen = h.IsOpen
        }).ToList<dynamic>();
    }

    public async Task<List<dynamic>> SetBusinessHoursAsync(Guid businessId, dynamic request)
    {
        // Extract hours from dynamic request
        List<dynamic> hoursRequest = request.Hours;

        // Business rule: Validate that all 7 days are provided
        var dayOfWeeks = hoursRequest.Select(h => (int)h.DayOfWeek).Distinct().ToList();
        if (dayOfWeeks.Count != 7 || !Enumerable.Range(0, 7).All(dayOfWeeks.Contains))
        {
            throw new ArgumentException("Must provide hours for all 7 days (0-6)");
        }

        // Business rule: Validate open/close times
        foreach (var hour in hoursRequest)
        {
            bool isOpen = hour.IsOpen;
            TimeOnly? openTime = hour.OpenTime;
            TimeOnly? closeTime = hour.CloseTime;
            int dayOfWeek = hour.DayOfWeek;

            if (isOpen && (openTime == null || closeTime == null))
            {
                throw new ArgumentException(
                    $"Day {dayOfWeek}: If open, both open and close times are required");
            }

            if (isOpen && openTime >= closeTime)
            {
                throw new ArgumentException(
                    $"Day {dayOfWeek}: Open time must be before close time");
            }
        }

        // Delete existing hours for this business
        var existingHours = await _businessHoursRepository.GetByBusinessIdAsync(businessId);
        _businessHoursRepository.DeleteRange(existingHours);

        // Create new hours
        var newHours = hoursRequest.Select(h => new BusinessHours
        {
            BusinessId = businessId,
            DayOfWeek = (DayOfWeek)(int)h.DayOfWeek,
            OpenTime = h.OpenTime,
            CloseTime = h.CloseTime,
            IsOpen = h.IsOpen
        }).ToList();

        await _businessHoursRepository.AddRangeAsync(newHours);
        await _businessHoursRepository.SaveChangesAsync();

        // Map to DTOs
        return newHours.Select(h => new
        {
            Id = h.Id,
            DayOfWeek = (int)h.DayOfWeek,
            DayName = h.DayOfWeek.ToString(),
            OpenTime = h.OpenTime,
            CloseTime = h.CloseTime,
            IsOpen = h.IsOpen
        }).OrderBy(h => h.DayOfWeek).ToList<dynamic>();
    }
}
