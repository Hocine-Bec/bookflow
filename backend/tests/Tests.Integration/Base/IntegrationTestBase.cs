using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Tests.Integration.Base;

/// <summary>
/// Base class for integration tests providing in-memory database setup and utilities.
/// </summary>
public abstract class IntegrationTestBase : IDisposable
{
    protected readonly AppDbContext Context;

    protected IntegrationTestBase()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        Context = new AppDbContext(options);
    }

    /// <summary>
    /// Seeds entities into the in-memory database and clears change tracker.
    /// </summary>
    protected async Task SeedAsync<T>(params T[] entities) where T : class
    {
        await Context.Set<T>().AddRangeAsync(entities);
        await Context.SaveChangesAsync();
        ClearTracking();
    }

    /// <summary>
    /// Manually clears the EF Core change tracker for clean entity state testing.
    /// </summary>
    protected void ClearTracking()
    {
        Context.ChangeTracker.Clear();
    }

    public void Dispose()
    {
        Context.Database.EnsureDeleted();
        Context.Dispose();
        GC.SuppressFinalize(this);
    }
}
