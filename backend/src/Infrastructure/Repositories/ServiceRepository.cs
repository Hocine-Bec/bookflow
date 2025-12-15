using Core.Entities;
using Core.Interfaces.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Service entity data access.
/// </summary>
public class ServiceRepository : IServiceRepository
{
    private readonly AppDbContext _context;

    public ServiceRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Service?> GetByIdAsync(Guid id)
    {
        return await _context.Services
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Service?> GetActiveByIdAsync(Guid id)
    {
        return await _context.Services
            .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);
    }

    public async Task<List<Service>> GetByBusinessIdAsync(Guid businessId)
    {
        return await _context.Services
            .Where(s => s.BusinessId == businessId)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task AddAsync(Service service)
    {
        await _context.Services.AddAsync(service);
    }

    public void Update(Service service)
    {
        _context.Services.Update(service);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
