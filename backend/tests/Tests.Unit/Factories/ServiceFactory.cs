using Bogus;
using Core.Entities;

namespace Tests.Unit.Factories;

/// <summary>
/// Factory for generating fake Service entities for testing using Bogus.
/// </summary>
public static class ServiceFactory
{
    private static readonly Faker _faker = new Faker();
    private static readonly string[] ServiceNames = {
        "Haircut", "Massage", "Consultation", "Training Session", "Cleaning",
        "Repair", "Installation", "Maintenance", "Design", "Photography"
    };

    public static Service CreateFakeService(
        Guid? id = null,
        Guid? businessId = null,
        string? name = null,
        string? description = null,
        int? durationMinutes = null,
        decimal? price = null,
        bool? isActive = null)
    {
        return new Service
        {
            Id = id ?? Guid.NewGuid(),
            BusinessId = businessId ?? Guid.NewGuid(),
            Name = name ?? _faker.PickRandom(ServiceNames),
            Description = description ?? _faker.Lorem.Sentence(),
            DurationMinutes = durationMinutes ?? _faker.Random.Int(15, 180),
            Price = price ?? _faker.Random.Decimal(10, 500),
            IsActive = isActive ?? true,
            CreatedAt = DateTime.UtcNow.AddDays(-_faker.Random.Int(1, 365)),
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static List<Service> CreateFakeServices(
        int count,
        Guid? businessId = null,
        bool allActive = true)
    {
        var services = new List<Service>();
        var busId = businessId ?? Guid.NewGuid();
        
        for (int i = 0; i < count; i++)
        {
            services.Add(CreateFakeService(
                businessId: busId,
                isActive: allActive || i % 2 == 0));
        }
        
        return services;
    }
}
