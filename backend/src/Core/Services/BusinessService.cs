using Core.Interfaces.Repositories;
using Core.Interfaces.Services;

namespace Core.Services;

/// <summary>
/// Business service for managing business profile operations.
/// Contains pure business logic with no direct database dependencies.
/// </summary>
public class BusinessService : IBusinessService
{
    private readonly IUserRepository _userRepository;
    private readonly IBusinessHoursRepository _businessHoursRepository;

    public BusinessService(
        IUserRepository userRepository,
        IBusinessHoursRepository businessHoursRepository)
    {
        _userRepository = userRepository;
        _businessHoursRepository = businessHoursRepository;
    }

    public async Task<dynamic?> GetBusinessBySlugAsync(string slug)
    {
        var user = await _userRepository.GetBySlugAsync(slug);
        
        if (user == null)
            return null;

        // Map to DTO
        return new
        {
            Id = user.Id,
            BusinessName = user.BusinessName,
            BusinessSlug = user.BusinessSlug,
            BusinessPhone = user.BusinessPhone,
            BusinessDescription = user.BusinessDescription,
            Services = user.Services
                .Where(s => s.IsActive)
                .Select(s => new
                {
                    Id = s.Id,
                    BusinessId = s.BusinessId,
                    Name = s.Name,
                    Description = s.Description,
                    DurationMinutes = s.DurationMinutes,
                    Price = s.Price,
                    IsActive = s.IsActive,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt
                })
                .OrderBy(s => s.Name)
                .ToList()
        };
    }

    public async Task<dynamic> GetBusinessHoursAsync(Guid businessId)
    {
        // Business rule: Check if business exists
        var businessExists = await _userRepository.ExistsAsync(businessId);
        if (!businessExists)
            return new List<object>(); // Return empty list instead of null

        var hours = await _businessHoursRepository.GetByBusinessIdAsync(businessId);

        // Map to DTO
        return hours.Select(h => new
        {
            Id = h.Id,
            DayOfWeek = (int)h.DayOfWeek,
            DayName = h.DayOfWeek.ToString(),
            OpenTime = h.OpenTime,
            CloseTime = h.CloseTime,
            IsOpen = h.IsOpen
        }).ToList();
    }
}
