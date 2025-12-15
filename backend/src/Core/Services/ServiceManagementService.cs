using Core.Entities;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;

namespace Core.Services;

/// <summary>
/// Business service for managing service CRUD operations.
/// Contains pure business logic with no direct database dependencies.
/// </summary>
public class ServiceManagementService : IServiceManagementService
{
    private readonly IServiceRepository _serviceRepository;

    public ServiceManagementService(IServiceRepository serviceRepository)
    {
        _serviceRepository = serviceRepository;
    }

    public async Task<dynamic?> GetServiceByIdAsync(Guid serviceId)
    {
        var service = await _serviceRepository.GetActiveByIdAsync(serviceId);
        
        if (service == null)
            return null;

        // Map to DTO
        return new
        {
            Id = service.Id,
            BusinessId = service.BusinessId,
            Name = service.Name,
            Description = service.Description,
            DurationMinutes = service.DurationMinutes,
            Price = service.Price,
            IsActive = service.IsActive,
            CreatedAt = service.CreatedAt,
            UpdatedAt = service.UpdatedAt
        };
    }

    public async Task<List<dynamic>> GetServicesByBusinessIdAsync(Guid businessId)
    {
        var services = await _serviceRepository.GetByBusinessIdAsync(businessId);

        // Map to DTOs
        return services.Select(s => new
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
        }).ToList<dynamic>();
    }

    public async Task<dynamic> CreateServiceAsync(Guid businessId, dynamic request)
    {
        // Business rule: Create service entity
        var service = new Service
        {
            BusinessId = businessId,
            Name = request.Name,
            Description = request.Description,
            DurationMinutes = request.DurationMinutes,
            Price = request.Price,
            IsActive = true
        };

        await _serviceRepository.AddAsync(service);
        await _serviceRepository.SaveChangesAsync();

        // Map to DTO
        return new
        {
            Id = service.Id,
            BusinessId = service.BusinessId,
            Name = service.Name,
            Description = service.Description,
            DurationMinutes = service.DurationMinutes,
            Price = service.Price,
            IsActive = service.IsActive,
            CreatedAt = service.CreatedAt,
            UpdatedAt = service.UpdatedAt
        };
    }

    public async Task<dynamic?> UpdateServiceAsync(Guid businessId, Guid serviceId, dynamic request)
    {
        var service = await _serviceRepository.GetByIdAsync(serviceId);

        // Business rule: Validate ownership
        if (service == null || service.BusinessId != businessId)
            return null;

        // Business rule: Update service properties
        service.Name = request.Name;
        service.Description = request.Description;
        service.DurationMinutes = request.DurationMinutes;
        service.Price = request.Price;
        service.IsActive = request.IsActive;

        _serviceRepository.Update(service);
        await _serviceRepository.SaveChangesAsync();

        // Map to DTO
        return new
        {
            Id = service.Id,
            BusinessId = service.BusinessId,
            Name = service.Name,
            Description = service.Description,
            DurationMinutes = service.DurationMinutes,
            Price = service.Price,
            IsActive = service.IsActive,
            CreatedAt = service.CreatedAt,
            UpdatedAt = service.UpdatedAt
        };
    }

    public async Task<bool> DeleteServiceAsync(Guid businessId, Guid serviceId)
    {
        var service = await _serviceRepository.GetByIdAsync(serviceId);

        // Business rule: Validate ownership
        if (service == null || service.BusinessId != businessId)
            return false;

        // Business rule: Soft delete - mark as inactive
        service.IsActive = false;
        
        _serviceRepository.Update(service);
        await _serviceRepository.SaveChangesAsync();

        return true;
    }
}
