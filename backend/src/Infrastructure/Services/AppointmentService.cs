using Core.Entities;
using Core.Enums;
using Core.Interfaces.Services;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class AppointmentService : IAppointmentService
{
    private readonly AppDbContext _context;

    public AppointmentService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<dynamic> GetAvailableSlotsAsync(Guid businessId, Guid serviceId, DateOnly date)
    {
        // Get the service to check duration
        var service = await _context.Services
            .FirstOrDefaultAsync(s => s.Id == serviceId && s.BusinessId == businessId && !s.IsDeleted);

        if (service == null)
            return new
            {
                ServiceId = serviceId,
                Date = date,
                ServiceName = (string?)null,
                DurationMinutes = 0,
                AvailableSlots = new List<object>()
            };

        // Get business hours for the requested date's day of week
        var dayOfWeek = date.DayOfWeek;
        var businessHours = await _context.BusinessHours
            .FirstOrDefaultAsync(bh => bh.BusinessId == businessId && bh.DayOfWeek == dayOfWeek && !bh.IsDeleted);

        if (businessHours == null || !businessHours.IsOpen || businessHours.OpenTime == null || businessHours.CloseTime == null)
            return new
            {
                ServiceId = serviceId,
                Date = date,
                ServiceName = service.Name,
                DurationMinutes = service.DurationMinutes,
                AvailableSlots = new List<object>()
            };

        // Query existing appointments for this business on the requested date (non-cancelled)
        var existingAppointments = await _context.Appointments
            .Where(a => a.BusinessId == businessId &&
                        a.AppointmentDate == date &&
                        a.Status != AppointmentStatus.Cancelled &&
                        !a.IsDeleted)
            .ToListAsync();

        // Generate 15-minute slots
        var availableSlots = new List<object>();
        var currentTime = businessHours.OpenTime.Value;

        while (currentTime.AddMinutes(service.DurationMinutes) <= businessHours.CloseTime.Value)
        {
            var slotEndTime = currentTime.AddMinutes(service.DurationMinutes);

            // Check if this slot conflicts with any existing appointment
            var hasConflict = existingAppointments.Any(a =>
                currentTime < a.EndTime && slotEndTime > a.StartTime);

            if (!hasConflict)
            {
                availableSlots.Add(new
                {
                    StartTime = currentTime,
                    EndTime = slotEndTime
                });
            }

            // Move to next 15-minute slot
            currentTime = currentTime.AddMinutes(15);
        }

        return new
        {
            ServiceId = serviceId,
            Date = date,
            ServiceName = service.Name,
            DurationMinutes = service.DurationMinutes,
            AvailableSlots = availableSlots
        };
    }

    public async Task<dynamic?> CreateAppointmentAsync(Guid businessId, dynamic request)
    {
        // Extract dynamic properties before using in queries
        Guid serviceId = request.ServiceId;
        string customerName = request.CustomerName;
        string customerEmail = request.CustomerEmail;
        string customerPhone = request.CustomerPhone;
        DateTime appointmentDateTime = request.AppointmentDate;
        string? notes = request.Notes;

        // Validate request date
        var requestDate = new DateOnly(appointmentDateTime.Year, appointmentDateTime.Month, appointmentDateTime.Day);
        var now = DateOnly.FromDateTime(DateTime.UtcNow);

        // Check minimum 2-hour notice
        var minimumBookingTime = DateTime.UtcNow.AddHours(2);
        if (appointmentDateTime < minimumBookingTime)
            return null;

        // Check maximum 90 days ahead
        var maxDate = now.AddDays(90);
        if (requestDate > maxDate)
            return null;

        // Get the service
        var service = await _context.Services
            .FirstOrDefaultAsync(s => s.Id == serviceId && s.BusinessId == businessId && !s.IsDeleted);

        if (service == null)
            return null;

        var requestStartTime = new TimeOnly(appointmentDateTime.Hour, appointmentDateTime.Minute);
        var requestEndTime = requestStartTime.AddMinutes(service.DurationMinutes);

        // Check if slot is available
        var isAvailable = await IsSlotAvailableAsync(businessId, serviceId, requestDate, requestStartTime, requestEndTime);
        if (!isAvailable)
            return null;

        // Create appointment
        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            ServiceId = serviceId,
            CustomerName = customerName,
            CustomerEmail = customerEmail,
            CustomerPhone = customerPhone,
            AppointmentDate = requestDate,
            StartTime = requestStartTime,
            EndTime = requestEndTime,
            Notes = notes,
            Status = AppointmentStatus.Confirmed,
            ConfirmationNumber = GenerateConfirmationNumber(),
            CancellationToken = GenerateCancellationToken(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        return new
        {
            Id = appointment.Id,
            ConfirmationNumber = appointment.ConfirmationNumber,
            ServiceName = service.Name,
            CustomerName = appointment.CustomerName,
            CustomerEmail = appointment.CustomerEmail,
            CustomerPhone = appointment.CustomerPhone,
            AppointmentDate = appointment.AppointmentDate,
            StartTime = appointment.StartTime,
            EndTime = appointment.EndTime,
            Status = appointment.Status.ToString(),
            Notes = appointment.Notes
        };
    }

    public async Task<List<dynamic>> GetAppointmentsAsync(Guid businessId, DateOnly? startDate, DateOnly? endDate, string? status)
    {
        var query = _context.Appointments
            .Where(a => a.BusinessId == businessId && !a.IsDeleted);

        if (startDate.HasValue)
            query = query.Where(a => a.AppointmentDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(a => a.AppointmentDate <= endDate.Value);

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<AppointmentStatus>(status, true, out var statusEnum))
            query = query.Where(a => a.Status == statusEnum);

        var appointments = await query
            .Include(a => a.Service)
            .OrderBy(a => a.AppointmentDate)
            .ThenBy(a => a.StartTime)
            .ToListAsync();

        return appointments.Select(a => new
        {
            Id = a.Id,
            ConfirmationNumber = a.ConfirmationNumber,
            ServiceName = a.Service!.Name,
            CustomerName = a.CustomerName,
            CustomerEmail = a.CustomerEmail,
            CustomerPhone = a.CustomerPhone,
            AppointmentDate = a.AppointmentDate,
            StartTime = a.StartTime,
            EndTime = a.EndTime,
            Status = a.Status.ToString(),
            Notes = a.Notes
        }).ToList<dynamic>();
    }

    public async Task<dynamic?> GetAppointmentByTokenAsync(string token)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Service)
            .Include(a => a.Business)
            .FirstOrDefaultAsync(a => a.CancellationToken == token && !a.IsDeleted);

        if (appointment == null)
            return null;

        var now = DateTime.UtcNow;
        var appointmentDateTime = appointment.AppointmentDate.ToDateTime(appointment.StartTime);

        return new
        {
            Id = appointment.Id,
            ConfirmationNumber = appointment.ConfirmationNumber,
            ServiceName = appointment.Service!.Name,
            BusinessName = appointment.Business!.BusinessName,
            BusinessPhone = appointment.Business.BusinessPhone,
            CustomerName = appointment.CustomerName,
            CustomerEmail = appointment.CustomerEmail,
            CustomerPhone = appointment.CustomerPhone,
            AppointmentDate = appointment.AppointmentDate,
            StartTime = appointment.StartTime,
            EndTime = appointment.EndTime,
            Status = appointment.Status.ToString(),
            Notes = appointment.Notes,
            CanCancel = appointmentDateTime > now
        };
    }

    public async Task<bool> CancelAppointmentAsync(string token)
    {
        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.CancellationToken == token && !a.IsDeleted);

        if (appointment == null || appointment.Status == AppointmentStatus.Cancelled)
            return false;

        // Check if appointment is in the future
        var appointmentDateTime = appointment.AppointmentDate.ToDateTime(appointment.StartTime);
        if (appointmentDateTime <= DateTime.UtcNow)
            return false;

        appointment.Status = AppointmentStatus.Cancelled;
        appointment.UpdatedAt = DateTime.UtcNow;

        _context.Appointments.Update(appointment);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<dynamic?> UpdateAppointmentStatusAsync(Guid businessId, Guid appointmentId, string status)
    {
        if (!Enum.TryParse<AppointmentStatus>(status, true, out var statusEnum))
            return null;

        var appointment = await _context.Appointments
            .Include(a => a.Service)
            .FirstOrDefaultAsync(a => a.Id == appointmentId && a.BusinessId == businessId && !a.IsDeleted);

        if (appointment == null)
            return null;

        appointment.Status = statusEnum;
        appointment.UpdatedAt = DateTime.UtcNow;

        _context.Appointments.Update(appointment);
        await _context.SaveChangesAsync();

        return new
        {
            Id = appointment.Id,
            ConfirmationNumber = appointment.ConfirmationNumber,
            ServiceName = appointment.Service!.Name,
            CustomerName = appointment.CustomerName,
            CustomerEmail = appointment.CustomerEmail,
            CustomerPhone = appointment.CustomerPhone,
            AppointmentDate = appointment.AppointmentDate,
            StartTime = appointment.StartTime,
            EndTime = appointment.EndTime,
            Status = appointment.Status.ToString(),
            Notes = appointment.Notes
        };
    }

    public async Task<bool> IsSlotAvailableAsync(Guid businessId, Guid serviceId, DateOnly date, TimeOnly startTime, TimeOnly endTime)
    {
        // Check business hours
        var dayOfWeek = date.DayOfWeek;
        var businessHours = await _context.BusinessHours
            .FirstOrDefaultAsync(bh => bh.BusinessId == businessId && bh.DayOfWeek == dayOfWeek && !bh.IsDeleted);

        if (businessHours == null || !businessHours.IsOpen || businessHours.OpenTime == null || businessHours.CloseTime == null)
            return false;

        if (startTime < businessHours.OpenTime.Value || endTime > businessHours.CloseTime.Value)
            return false;

        // Check for conflicts with existing appointments
        var hasConflict = await _context.Appointments
            .AnyAsync(a => a.BusinessId == businessId &&
                          a.ServiceId == serviceId &&
                          a.AppointmentDate == date &&
                          a.Status != AppointmentStatus.Cancelled &&
                          !a.IsDeleted &&
                          startTime < a.EndTime &&
                          endTime > a.StartTime);

        return !hasConflict;
    }

    public string GenerateConfirmationNumber()
    {
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        var random = new Random();
        var randomChars = new string(Enumerable.Range(0, 6)
            .Select(_ => "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"[random.Next(36)])
            .ToArray());

        return $"BK-{date}-{randomChars}";
    }

    public string GenerateCancellationToken()
    {
        return Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
    }
}
