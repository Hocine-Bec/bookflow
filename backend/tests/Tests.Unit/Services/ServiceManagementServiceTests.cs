using Core.Entities;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Core.Services;
using FluentAssertions;
using Moq;
using Tests.Unit.Factories;
using Xunit;

namespace Tests.Unit.Services;

/// <summary>
/// Unit tests for ServiceManagementService - tests CRUD business logic with mocked repositories.
/// </summary>
public class ServiceManagementServiceTests
{
    private readonly Mock<IServiceRepository> _mockServiceRepo;
    private readonly ServiceManagementService _service;

    public ServiceManagementServiceTests()
    {
        _mockServiceRepo = new Mock<IServiceRepository>();
        _service = new ServiceManagementService(_mockServiceRepo.Object);
    }

    #region GetServiceByIdAsync Tests

    [Fact]
    public async Task GetServiceByIdAsync_ExistingActiveService_ReturnsDTO()
    {
        // Arrange
        var serviceEntity = ServiceFactory.CreateFakeService(isActive: true);
        _mockServiceRepo.Setup(r => r.GetActiveByIdAsync(serviceEntity.Id))
            .ReturnsAsync(serviceEntity);

        // Act
        var result = await _service.GetServiceByIdAsync(serviceEntity.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(serviceEntity.Id);
        result.Name.Should().Be(serviceEntity.Name);
    }

    [Fact]
    public async Task GetServiceByIdAsync_InactiveService_ReturnsNull()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        _mockServiceRepo.Setup(r => r.GetActiveByIdAsync(serviceId))
            .ReturnsAsync((Service?)null);

        // Act
        var result = await _service.GetServiceByIdAsync(serviceId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetServiceByIdAsync_NonExistingService_ReturnsNull()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        _mockServiceRepo.Setup(r => r.GetActiveByIdAsync(serviceId))
            .ReturnsAsync((Service?)null);

        // Act
        var result = await _service.GetServiceByIdAsync(serviceId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region CreateServiceAsync Tests

    [Fact]
    public async Task CreateServiceAsync_ValidService_CreatesSuccessfully()
    {
        // Arrange
        var businessId = Guid.NewGuid();
        dynamic request = new
        {
            Name = "Test Service",
            Description = "Test Description",
            DurationMinutes = 60,
            Price = 100.00m
        };

        _mockServiceRepo.Setup(r => r.AddAsync(It.IsAny<Service>()))
            .Returns(Task.CompletedTask);
        _mockServiceRepo.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateServiceAsync(businessId, request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Test Service");
        result.IsActive.Should().BeTrue();
        result.BusinessId.Should().Be(businessId);
    }

    [Fact]
    public async Task CreateServiceAsync_CallsRepositoryAddAsync()
    {
        // Arrange
        var businessId = Guid.NewGuid();
        dynamic request = new { Name = "Test", Description = "Desc", DurationMinutes = 30, Price = 50m };

        // Act
        await _service.CreateServiceAsync(businessId, request);

        // Assert
        _mockServiceRepo.Verify(r => r.AddAsync(It.IsAny<Service>()), Times.Once);
    }

    [Fact]
    public async Task CreateServiceAsync_CallsRepositorySaveChangesAsync()
    {
        // Arrange
        var businessId = Guid.NewGuid();
        dynamic request = new { Name = "Test", Description = "Desc", DurationMinutes = 30, Price = 50m };

        // Act
        await _service.CreateServiceAsync(businessId, request);

        // Assert
        _mockServiceRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    #endregion

    #region UpdateServiceAsync Tests

    [Fact]
    public async Task UpdateServiceAsync_ExistingService_UpdatesSuccessfully()
    {
        // Arrange
        var businessId = Guid.NewGuid();
        var serviceEntity = ServiceFactory.CreateFakeService(businessId: businessId);
        dynamic request = new
        {
            Name = "Updated Name",
            Description = "Updated Description",
            DurationMinutes = 90,
            Price = 150.00m,
            IsActive = true
        };

        _mockServiceRepo.Setup(r => r.GetByIdAsync(serviceEntity.Id))
            .ReturnsAsync(serviceEntity);

        // Act
        var result = await _service.UpdateServiceAsync(businessId, serviceEntity.Id, request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Name");
        _mockServiceRepo.Verify(r => r.Update(It.IsAny<Service>()), Times.Once);
        _mockServiceRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateServiceAsync_WrongBusinessId_ReturnsNull()
    {
        // Arrange
        var correctBusinessId = Guid.NewGuid();
        var wrongBusinessId = Guid.NewGuid();
        var serviceEntity = ServiceFactory.CreateFakeService(businessId: correctBusinessId);
        dynamic request = new { Name = "Test", Description = "Desc", DurationMinutes = 30, Price = 50m, IsActive = true };

        _mockServiceRepo.Setup(r => r.GetByIdAsync(serviceEntity.Id))
            .ReturnsAsync(serviceEntity);

        // Act
        var result = await _service.UpdateServiceAsync(wrongBusinessId, serviceEntity.Id, request);

        // Assert
        result.Should().BeNull();
        _mockServiceRepo.Verify(r => r.Update(It.IsAny<Service>()), Times.Never);
    }

    [Fact]
    public async Task UpdateServiceAsync_NonExistingService_ReturnsNull()
    {
        // Arrange
        var businessId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        dynamic request = new { Name = "Test", Description = "Desc", DurationMinutes = 30, Price = 50m, IsActive = true };

        _mockServiceRepo.Setup(r => r.GetByIdAsync(serviceId))
            .ReturnsAsync((Service?)null);

        // Act
        var result = await _service.UpdateServiceAsync(businessId, serviceId, request);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region DeleteServiceAsync Tests

    [Fact]
    public async Task DeleteServiceAsync_ExistingService_SoftDeletesSuccessfully()
    {
        // Arrange
        var businessId = Guid.NewGuid();
        var serviceEntity = ServiceFactory.CreateFakeService(businessId: businessId, isActive: true);

        _mockServiceRepo.Setup(r => r.GetByIdAsync(serviceEntity.Id))
            .ReturnsAsync(serviceEntity);

        // Act
        var result = await _service.DeleteServiceAsync(businessId, serviceEntity.Id);

        // Assert
        result.Should().BeTrue();
        serviceEntity.IsActive.Should().BeFalse();
        _mockServiceRepo.Verify(r => r.Update(It.IsAny<Service>()), Times.Once);
        _mockServiceRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteServiceAsync_WrongBusinessId_ReturnsFalse()
    {
        // Arrange
        var correctBusinessId = Guid.NewGuid();
        var wrongBusinessId = Guid.NewGuid();
        var serviceEntity = ServiceFactory.CreateFakeService(businessId: correctBusinessId);

        _mockServiceRepo.Setup(r => r.GetByIdAsync(serviceEntity.Id))
            .ReturnsAsync(serviceEntity);

        // Act
        var result = await _service.DeleteServiceAsync(wrongBusinessId, serviceEntity.Id);

        // Assert
        result.Should().BeFalse();
        _mockServiceRepo.Verify(r => r.Update(It.IsAny<Service>()), Times.Never);
    }

    [Fact]
    public async Task DeleteServiceAsync_NonExistingService_ReturnsFalse()
    {
        // Arrange
        var businessId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();

        _mockServiceRepo.Setup(r => r.GetByIdAsync(serviceId))
            .ReturnsAsync((Service?)null);

        // Act
        var result = await _service.DeleteServiceAsync(businessId, serviceId);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
