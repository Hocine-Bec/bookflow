using Core.Entities;
using Mapster;
using WebApi.DTOs.Appointments;

namespace WebApi.Mappings;

/// <summary>
/// Mapster configuration for Appointment entity to DTO mappings.
/// Defines bidirectional mappings with custom property handling.
/// </summary>
public class AppointmentMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Appointment entity to DTO mappings
        config.NewConfig<Appointment, AppointmentResponse>()
            .Map(dest => dest.ServiceName, src => src.Service!.Name)
            .Map(dest => dest.Status, src => src.Status.ToString());

        config.NewConfig<Appointment, AppointmentDetailsResponse>()
            .Map(dest => dest.ServiceName, src => src.Service!.Name)
            .Map(dest => dest.BusinessName, src => src.Business!.BusinessName)
            .Map(dest => dest.BusinessPhone, src => src.Business!.BusinessPhone)
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.CanCancel, 
                src => src.AppointmentDate.ToDateTime(src.StartTime) > DateTime.UtcNow);

        config.NewConfig<Appointment, AvailableSlotResponse>()
            .Map(dest => dest.StartTime, src => src.StartTime)
            .Map(dest => dest.EndTime, src => src.EndTime);
    }
}

/// <summary>
/// Mapster configuration for Service entity to DTO mappings.
/// </summary>
public class ServiceMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Service, dynamic>()
            .MapWith(src => new
            {
                src.Id,
                src.Name,
                src.Description,
                src.DurationMinutes,
                src.Price,
                src.IsActive
            });
    }
}

/// <summary>
/// Mapster configuration for User entity to DTO mappings.
/// </summary>
public class UserMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<User, dynamic>()
            .MapWith(src => new
            {
                src.Id,
                src.Email,
                src.BusinessName,
                src.BusinessSlug,
                src.BusinessPhone,
                src.BusinessDescription,
                src.IsEmailVerified
            });
    }
}
