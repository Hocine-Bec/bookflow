using Bogus;
using Core.Entities;

namespace Tests.Unit.Factories;

/// <summary>
/// Factory for generating fake User entities for testing using Bogus.
/// </summary>
public static class UserFactory
{
    private static readonly Faker _faker = new Faker();

    public static User CreateFakeUser(
        Guid? id = null,
        string? businessName = null,
        string? businessSlug = null,
        string? businessPhone = null,
        string? businessDescription = null,
        string? email = null,
        List<Service>? services = null,
        List<BusinessHours>? businessHours = null)
    {
        var userId = id ?? Guid.NewGuid();
        var name = businessName ?? _faker.Company.CompanyName();
        
        return new User
        {
            Id = userId,
            Email = email ?? _faker.Internet.Email(),
            PasswordHash = _faker.Random.Hash(),
            BusinessName = name,
            BusinessSlug = businessSlug ?? _faker.Internet.DomainWord(),
            BusinessPhone = businessPhone ?? _faker.Phone.PhoneNumber("###-###-####"),
            BusinessDescription = businessDescription ?? _faker.Company.CatchPhrase(),
            Services = services ?? new List<Service>(),
            BusinessHours = businessHours ?? new List<BusinessHours>()
        };
    }

    public static User CreateFakeUserWithServices(
        Guid? id = null,
        int serviceCount = 3,
        bool allActive = true)
    {
        var user = CreateFakeUser(id: id);
        var services = new List<Service>();
        
        for (int i = 0; i < serviceCount; i++)
        {
            services.Add(ServiceFactory.CreateFakeService(
                businessId: user.Id,
                isActive: allActive || i % 2 == 0));
        }
        
        user.Services = services;
        return user;
    }

    public static User CreateFakeUserWithBusinessHours(
        Guid? id = null,
        bool allOpen = true)
    {
        var user = CreateFakeUser(id: id);
        user.BusinessHours = BusinessHoursFactory.CreateFakeBusinessHoursForWeek(
            businessId: user.Id,
            allOpen: allOpen);
        
        return user;
    }
}
