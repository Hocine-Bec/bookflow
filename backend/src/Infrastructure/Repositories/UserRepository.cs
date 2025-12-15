using Core.Entities;
using Core.Interfaces.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

/// <summary>
/// Repository implementation for User entity data access.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetBySlugAsync(string slug)
    {
        return await _context.Users
            .Where(u => u.BusinessSlug.ToLower() == slug.ToLower())
            .Include(u => u.Services.Where(s => s.IsActive))
            .Include(u => u.BusinessHours)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> ExistsAsync(Guid userId)
    {
        return await _context.Users.AnyAsync(u => u.Id == userId);
    }
}
