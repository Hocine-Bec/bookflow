using Core.Entities;

namespace Core.Interfaces.Services;

public interface IAppointmentService
{
    // Returns available slots (DTO mapping handled in WebApi layer)
    Task<dynamic> GetAvailableSlotsAsync(Guid businessId, Guid serviceId, DateOnly date);
    
    // Creates appointment and returns appointment details (DTO mapping handled in WebApi layer)
    Task<dynamic?> CreateAppointmentAsync(Guid businessId, dynamic request);
    
    // Gets all appointments for a business (DTO mapping handled in WebApi layer)
    Task<List<dynamic>> GetAppointmentsAsync(Guid businessId, DateOnly? startDate, DateOnly? endDate, string? status);
    
    // Gets appointment by cancellation token
    Task<dynamic?> GetAppointmentByTokenAsync(string token);
    
    // Cancels appointment using token
    Task<bool> CancelAppointmentAsync(string token);
    
    // Updates appointment status by business owner
    Task<dynamic?> UpdateAppointmentStatusAsync(Guid businessId, Guid appointmentId, string status);
    
    // Checks if a time slot is available
    Task<bool> IsSlotAvailableAsync(Guid businessId, Guid serviceId, DateOnly date, TimeOnly startTime, TimeOnly endTime);
    
    // Generates confirmation number
    string GenerateConfirmationNumber();
    
    // Generates cancellation token
    string GenerateCancellationToken();
}
