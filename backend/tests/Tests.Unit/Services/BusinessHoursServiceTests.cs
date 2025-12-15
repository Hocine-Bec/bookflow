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
/// Unit tests for BusinessHoursService - tests business hours validation logic with mocked repositories.
/// </summary>
public class BusinessHoursServiceTests
{
    private readonly Mock<IBusinessHoursRepository> _mockBusinessHoursRepo;
    private readonly BusinessHoursService _service;

    public BusinessHoursServiceTests()
    {
        _mockBusinessHoursRepo = new Mock<IBusinessHoursRepository>();
        _service = new BusinessHoursService(_mockBusinessHoursRepo.Object);
    }

    #region GetBusinessHoursByBusinessIdAsync Tests

    [Fact]
    public async Task GetBusinessHoursByBusinessIdAsync_ReturnsAllHours()
    {
        // Arrange
        var businessId = Guid.NewGuid();
        var hours = BusinessHoursFactory.CreateFakeBusinessHoursForWeek(businessId: businessId);
        _mockBusinessHoursRepo.Setup(r => r.GetByBusinessIdAsync(businessId))
            .ReturnsAsync(hours);

        // Act
        var result = await _service.GetBusinessHoursByBusinessIdAsync(businessId);

        // Assert
        var hoursList = result as List<dynamic>;
        hoursList.Should().HaveCount(7);
    }

    [Fact]
    public async Task GetBusinessHoursByBusinessIdAsync_EmptyList_ReturnsEmptyList()
    {
        // Arrange
        var businessId = Guid.NewGuid();
        _mockBusinessHoursRepo.Setup(r => r.GetByBusinessIdAsync(businessId))
            .ReturnsAsync(new List<BusinessHours>());

        // Act
        var result = await _service.GetBusinessHoursByBusinessIdAsync(businessId);

        // Assert
        var hoursList = result as List<dynamic>;
        hoursList.Should().BeEmpty();
    }

    #endregion

    #region SetBusinessHoursAsync Tests

    [Fact]
    public async Task SetBusinessHoursAsync_ValidHours_SavesSuccessfully()
    {
        // Arrange
        var businessId = Guid.NewGuid();
        var hoursRequest = CreateValidBusinessHoursRequest();
        dynamic request = new { Hours = hoursRequest };

        _mockBusinessHoursRepo.Setup(r => r.GetByBusinessIdAsync(businessId))
            .ReturnsAsync(new List<BusinessHours>());

        // Act
        var result = await _service.SetBusinessHoursAsync(businessId, request);

        // Assert
        var hoursList = result as List<dynamic>;
        hoursList.Should().HaveCount(7);
        _mockBusinessHoursRepo.Verify(r => r.DeleteRange(It.IsAny<IEnumerable<BusinessHours>>()), Times.Once);
        _mockBusinessHoursRepo.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<BusinessHours>>()), Times.Once);
        _mockBusinessHoursRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task SetBusinessHoursAsync_LessThan7Days_ThrowsException()
    {
        // Arrange
        var businessId = Guid.NewGuid();
        var hoursRequest = CreateValidBusinessHoursRequest().Take(6).ToList();
        dynamic request = new { Hours = hoursRequest };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _service.SetBusinessHoursAsync(businessId, request));
    }

    [Fact]
    public async Task SetBusinessHoursAsync_MoreThan7Days_ThrowsException()
    {
        // Arrange
        var businessId = Guid.NewGuid();
        var hoursRequest = CreateValidBusinessHoursRequest();
        hoursRequest.Add(new { DayOfWeek = 0, IsOpen = true, OpenTime = new TimeOnly(9, 0), CloseTime = new TimeOnly(17, 0) });
        dynamic request = new { Hours = hoursRequest };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _service.SetBusinessHoursAsync(businessId, request));
    }

    [Fact]
    public async Task SetBusinessHoursAsync_IsOpenButNoOpenTime_ThrowsException()
    {
        // Arrange
        var businessId = Guid.NewGuid();
        var hoursRequest = CreateValidBusinessHoursRequest();
        hoursRequest[0] = new { DayOfWeek = 0, IsOpen = true, OpenTime = (TimeOnly?)null, CloseTime = new TimeOnly(17, 0) };
        dynamic request = new { Hours = hoursRequest };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            async () => await _service.SetBusinessHoursAsync(businessId, request));
        exception.Message.Should().Contain("open and close times are required");
    }

    [Fact]
    public async Task SetBusinessHoursAsync_IsOpenButNoCloseTime_ThrowsException()
    {
        // Arrange
        var businessId = Guid.NewGuid();
        var hoursRequest = CreateValidBusinessHoursRequest();
        hoursRequest[0] = new { DayOfWeek = 0, IsOpen = true, OpenTime = new TimeOnly(9, 0), CloseTime = (TimeOnly?)null };
        dynamic request = new { Hours = hoursRequest };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            async () => await _service.SetBusinessHoursAsync(businessId, request));
        exception.Message.Should().Contain("open and close times are required");
    }

    [Fact]
    public async Task SetBusinessHoursAsync_OpenTimeAfterCloseTime_ThrowsException()
    {
        // Arrange
        var businessId = Guid.NewGuid();
        var hoursRequest = CreateValidBusinessHoursRequest();
        hoursRequest[0] = new { DayOfWeek = 0, IsOpen = true, OpenTime = new TimeOnly(17, 0), CloseTime = new TimeOnly(9, 0) };
        dynamic request = new { Hours = hoursRequest };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            async () => await _service.SetBusinessHoursAsync(businessId, request));
        exception.Message.Should().Contain("Open time must be before close time");
    }

    [Fact]
    public async Task SetBusinessHoursAsync_DeletesExistingHours()
    {
        // Arrange
        var businessId = Guid.NewGuid();
        var existingHours = BusinessHoursFactory.CreateFakeBusinessHoursForWeek(businessId: businessId);
        var hoursRequest = CreateValidBusinessHoursRequest();
        dynamic request = new { Hours = hoursRequest };

        _mockBusinessHoursRepo.Setup(r => r.GetByBusinessIdAsync(businessId))
            .ReturnsAsync(existingHours);

        // Act
        await _service.SetBusinessHoursAsync(businessId, request);

        // Assert
        _mockBusinessHoursRepo.Verify(r => r.DeleteRange(It.Is<IEnumerable<BusinessHours>>(
            h => h.Count() == 7)), Times.Once);
    }

    #endregion

    #region Helper Methods

    private List<dynamic> CreateValidBusinessHoursRequest()
    {
        var hours = new List<dynamic>();
        for (int i = 0; i < 7; i++)
        {
            hours.Add(new
            {
                DayOfWeek = i,
                IsOpen = i >= 1 && i <= 5, // Mon-Fri open
                OpenTime = (i >= 1 && i <= 5) ? new TimeOnly(9, 0) : (TimeOnly?)null,
                CloseTime = (i >= 1 && i <= 5) ? new TimeOnly(17, 0) : (TimeOnly?)null
            });
        }
        return hours;
    }

    #endregion
}
