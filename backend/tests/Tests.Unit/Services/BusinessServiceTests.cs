using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Core.Services;
using FluentAssertions;
using Moq;
using Tests.Unit.Factories;
using Xunit;

namespace Tests.Unit.Services;

/// <summary>
/// Unit tests for BusinessService - tests business logic with mocked repositories.
/// </summary>
public class BusinessServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<IBusinessHoursRepository> _mockBusinessHoursRepo;
    private readonly BusinessService _service;

    public BusinessServiceTests()
    {
        _mockUserRepo = new Mock<IUserRepository>();
        _mockBusinessHoursRepo = new Mock<IBusinessHoursRepository>();
        _service = new BusinessService(_mockUserRepo.Object, _mockBusinessHoursRepo.Object);
    }

    #region GetBusinessBySlugAsync Tests

    [Fact]
    public async Task GetBusinessBySlugAsync_ExistingSlug_ReturnsBusinessProfile()
    {
        // Arrange
        var user = UserFactory.CreateFakeUserWithServices(serviceCount: 2, allActive: true);
        _mockUserRepo.Setup(r => r.GetBySlugAsync(It.IsAny<string>()))
            .ReturnsAsync(user);

        // Act
        var result = await _service.GetBusinessBySlugAsync(user.BusinessSlug!);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id);
        result.BusinessName.Should().Be(user.BusinessName);
    }

    [Fact]
    public async Task GetBusinessBySlugAsync_NonExistingSlug_ReturnsNull()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.GetBySlugAsync(It.IsAny<string>()))
            .ReturnsAsync((Core.Entities.User?)null);

        // Act
        var result = await _service.GetBusinessBySlugAsync("non-existing-slug");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBusinessBySlugAsync_CaseInsensitive_CallsRepositoryWithSlug()
    {
        // Arrange
        var slug = "MyBusiness";
        var user = UserFactory.CreateFakeUser(businessSlug: slug.ToLower());
        _mockUserRepo.Setup(r => r.GetBySlugAsync(It.IsAny<string>()))
            .ReturnsAsync(user);

        // Act
        await _service.GetBusinessBySlugAsync(slug);

        // Assert
        _mockUserRepo.Verify(r => r.GetBySlugAsync(slug), Times.Once);
    }

    [Fact]
    public async Task GetBusinessBySlugAsync_IncludesActiveServicesOnly()
    {
        // Arrange
        var user = UserFactory.CreateFakeUser();
        user.Services = new List<Core.Entities.Service>
        {
            ServiceFactory.CreateFakeService(businessId: user.Id, name: "Active Service", isActive: true),
            ServiceFactory.CreateFakeService(businessId: user.Id, name: "Inactive Service", isActive: false)
        };
        _mockUserRepo.Setup(r => r.GetBySlugAsync(It.IsAny<string>()))
            .ReturnsAsync(user);

        // Act
        var result = await _service.GetBusinessBySlugAsync(user.BusinessSlug!);

        // Assert
        var services = result.Services as List<dynamic>;
        services.Should().HaveCount(1);
        services![0].Name.Should().Be("Active Service");
    }

    [Fact]
    public async Task GetBusinessBySlugAsync_MapsAllDTOFieldsCorrectly()
    {
        // Arrange
        var user = UserFactory.CreateFakeUser();
        _mockUserRepo.Setup(r => r.GetBySlugAsync(It.IsAny<string>()))
            .ReturnsAsync(user);

        // Act
        var result = await _service.GetBusinessBySlugAsync(user.BusinessSlug!);

        // Assert
        result.Id.Should().Be(user.Id);
        result.BusinessName.Should().Be(user.BusinessName);
        result.BusinessSlug.Should().Be(user.BusinessSlug);
        result.BusinessPhone.Should().Be(user.BusinessPhone);
        result.BusinessDescription.Should().Be(user.BusinessDescription);
    }

    [Fact]
    public async Task GetBusinessBySlugAsync_OrdersServicesByName()
    {
        // Arrange
        var user = UserFactory.CreateFakeUser();
        user.Services = new List<Core.Entities.Service>
        {
            ServiceFactory.CreateFakeService(businessId: user.Id, name: "Zebra Service", isActive: true),
            ServiceFactory.CreateFakeService(businessId: user.Id, name: "Alpha Service", isActive: true),
            ServiceFactory.CreateFakeService(businessId: user.Id, name: "Beta Service", isActive: true)
        };
        _mockUserRepo.Setup(r => r.GetBySlugAsync(It.IsAny<string>()))
            .ReturnsAsync(user);

        // Act
        var result = await _service.GetBusinessBySlugAsync(user.BusinessSlug!);

        // Assert
        var services = result.Services as List<dynamic>;
        services![0].Name.Should().Be("Alpha Service");
        services[1].Name.Should().Be("Beta Service");
        services[2].Name.Should().Be("Zebra Service");
    }

    #endregion

    #region GetBusinessHoursAsync Tests

    [Fact]
    public async Task GetBusinessHoursAsync_ExistingBusiness_ReturnsHours()
    {
        // Arrange
        var businessId = Guid.NewGuid();
        var hours = BusinessHoursFactory.CreateFakeBusinessHoursForWeek(businessId: businessId);
        _mockUserRepo.Setup(r => r.ExistsAsync(businessId)).ReturnsAsync(true);
        _mockBusinessHoursRepo.Setup(r => r.GetByBusinessIdAsync(businessId))
            .ReturnsAsync(hours);

        // Act
        var result = await _service.GetBusinessHoursAsync(businessId);

        // Assert
        var hoursList = result as List<dynamic>;
        hoursList.Should().HaveCount(7);
    }

    [Fact]
    public async Task GetBusinessHoursAsync_NonExistingBusiness_ReturnsEmptyList()
    {
        // Arrange
        var businessId = Guid.NewGuid();
        _mockUserRepo.Setup(r => r.ExistsAsync(businessId)).ReturnsAsync(false);

        // Act
        var result = await _service.GetBusinessHoursAsync(businessId);

        // Assert
        var hoursList = result as List<object>;
        hoursList.Should().BeEmpty();
    }

    [Fact]
    public async Task GetBusinessHoursAsync_MapsAllDTOFieldsCorrectly()
    {
        // Arrange
        var businessId = Guid.NewGuid();
        var hours = BusinessHoursFactory.CreateFakeBusinessHoursForWeek(businessId: businessId);
        _mockUserRepo.Setup(r => r.ExistsAsync(businessId)).ReturnsAsync(true);
        _mockBusinessHoursRepo.Setup(r => r.GetByBusinessIdAsync(businessId))
            .ReturnsAsync(hours);

        // Act
        var result = await _service.GetBusinessHoursAsync(businessId);

        // Assert
        var hoursList = result as List<dynamic>;
        var firstHour = hoursList![0];
        firstHour.Id.Should().NotBeEmpty();
        firstHour.DayOfWeek.Should().BeOfType<int>();
        firstHour.DayName.Should().NotBeNullOrEmpty();
        firstHour.IsOpen.Should().BeOfType<bool>();
    }

    [Fact]
    public async Task GetBusinessHoursAsync_BusinessWithNoHours_ReturnsEmptyList()
    {
        // Arrange
        var businessId = Guid.NewGuid();
        _mockUserRepo.Setup(r => r.ExistsAsync(businessId)).ReturnsAsync(true);
        _mockBusinessHoursRepo.Setup(r => r.GetByBusinessIdAsync(businessId))
            .ReturnsAsync(new List<Core.Entities.BusinessHours>());

        // Act
        var result = await _service.GetBusinessHoursAsync(businessId);

        // Assert
        var hoursList = result as List<dynamic>;
        hoursList.Should().BeEmpty();
    }

    #endregion
}
