using Core.Entities;
using Core.Interfaces.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

/// <summary>
/// Repository implementation for BusinessHours entity data access.
/// </summary>
public class BusinessHoursRepository : IBusinessHoursRepository
{
    private readonly AppDbContext _context;

    public BusinessHoursRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<BusinessHours>> GetByBusinessIdAsync(Guid businessId)
    {
        return await _context.BusinessHours
            .Where(h => h.BusinessId == businessId)
            .OrderBy(h => h.DayOfWeek)
            .ToListAsync();
    }

    public void DeleteRange(IEnumerable<BusinessHours> hours)
    {
        _context.BusinessHours.RemoveRange(hours);
    }

    public async Task AddRangeAsync(IEnumerable<BusinessHours> hours)
    {
        await _context.BusinessHours.AddRangeAsync(hours);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
