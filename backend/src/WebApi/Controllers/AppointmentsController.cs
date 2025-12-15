using Core.Common;
using Core.Interfaces.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApi.DTOs.Appointments;

namespace WebApi.Controllers;

/// <summary>
/// Manages appointment booking operations including availability checks,
/// booking creation, and cancellation for both customers and business owners.
/// Follows RESTful principles and implements proper error handling with Result pattern.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;
    private readonly IValidator<CreateAppointmentRequest> _createValidator;
    private readonly IValidator<UpdateAppointmentStatusRequest> _updateValidator;
    private readonly ILogger<AppointmentsController> _logger;

    public AppointmentsController(
        IAppointmentService appointmentService,
        IValidator<CreateAppointmentRequest> createValidator,
        IValidator<UpdateAppointmentStatusRequest> updateValidator,
        ILogger<AppointmentsController> logger)
    {
        _appointmentService = appointmentService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    /// <summary>
    /// Extracts the authenticated user ID from JWT claims.
    /// </summary>
    /// <exception cref="UnauthorizedAccessException">Thrown when user ID claim is invalid or missing</exception>
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            _logger.LogWarning("Invalid or missing user ID in authentication token");
            throw new UnauthorizedAccessException("Invalid or missing user ID in token");
        }

        return userId;
    }

    /// <summary>
    /// Retrieves available time slots for a specific service on a given date.
    /// Public endpoint for customers to check availability.
    /// </summary>
    /// <remarks>
    /// Available slots are calculated based on:
    /// - Service duration in minutes
    /// - Business hours for the requested day
    /// - Existing confirmed appointments (excluding cancelled)
    /// - 15-minute increment slots
    /// </remarks>
    /// <param name="serviceId">The service ID to check availability for</param>
    /// <param name="date">The date to check slots for</param>
    /// <returns>List of available time slots with service details</returns>
    [HttpGet("available-slots")]
    [ProducesResponseType(typeof(AvailableSlotsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AvailableSlotsResponse>> GetAvailableSlots(
        [FromQuery] Guid serviceId,
        [FromQuery] DateOnly date)
    {
        if (serviceId == Guid.Empty)
            return BadRequest(new { error = "ServiceId is required" });

        if (date == DateOnly.MinValue)
            return BadRequest(new { error = "Date is required" });

        try
        {
            var slots = await _appointmentService.GetAvailableSlotsAsync(Guid.Empty, serviceId, date);
            return Ok(slots);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available slots for service {ServiceId} on {Date}", 
                serviceId, date);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while fetching available slots" });
        }
    }

    /// <summary>
    /// Creates a new appointment booking. Validates availability, business hours, and conflict.
    /// Public endpoint - no authentication required for customer bookings.
    /// </summary>
    /// <remarks>
    /// Validation includes:
    /// - 2-hour minimum advance booking notice
    /// - 90-day maximum booking window
    /// - Valid customer information (email, phone)
    /// - No conflicts with existing appointments
    /// </remarks>
    /// <param name="request">Appointment booking details</param>
    /// <returns>Booking confirmation with confirmation number and cancellation token</returns>
    [HttpPost]
    [ProducesResponseType(typeof(BookingConfirmationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BookingConfirmationResponse>> CreateAppointment(
        [FromBody] CreateAppointmentRequest request)
    {
        // Validate request
        var validationResult = await _createValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return BadRequest(new { errors });
        }

        try
        {
            var appointment = await _appointmentService.CreateAppointmentAsync(Guid.Empty, request);

            if (appointment == null)
                return Conflict(new { error = "Unable to create appointment. Time slot may not be available or already booked." });

            return CreatedAtAction(nameof(GetAppointmentByToken), 
                new { token = "will-be-provided" }, 
                appointment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating appointment for {Email}", request.CustomerEmail);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while creating the appointment" });
        }
    }

    /// <summary>
    /// Retrieves all appointments for the authenticated business owner.
    /// Supports filtering by date range and status.
    /// </summary>
    /// <remarks>
    /// Requires authentication as a business owner.
    /// Results are filtered by the business owner's ID from the JWT token.
    /// </remarks>
    /// <param name="startDate">Optional start date filter (inclusive)</param>
    /// <param name="endDate">Optional end date filter (inclusive)</param>
    /// <param name="status">Optional status filter (e.g., Pending, Confirmed, Completed, Cancelled)</param>
    /// <returns>List of appointments matching the criteria</returns>
    [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(List<AppointmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<AppointmentResponse>>> GetAppointments(
        [FromQuery] DateOnly? startDate,
        [FromQuery] DateOnly? endDate,
        [FromQuery] string? status)
    {
        try
        {
            var businessId = GetCurrentUserId();
            var appointments = await _appointmentService.GetAppointmentsAsync(businessId, startDate, endDate, status);
            return Ok(appointments);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { error = "Invalid authentication credentials" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving appointments");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while retrieving appointments" });
        }
    }

    /// <summary>
    /// Retrieves appointment details using a cancellation token.
    /// Public endpoint - customers use this to view their appointment before cancelling.
    /// </summary>
    /// <param name="token">The cancellation token provided during booking</param>
    /// <returns>Appointment details including cancellation eligibility</returns>
    [HttpGet("by-token")]
    [ProducesResponseType(typeof(AppointmentDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AppointmentDetailsResponse>> GetAppointmentByToken(
        [FromQuery] string token)
    {
        if (string.IsNullOrEmpty(token))
            return BadRequest(new { error = "Cancellation token is required" });

        try
        {
            var appointment = await _appointmentService.GetAppointmentByTokenAsync(token);

            if (appointment == null)
                return NotFound(new { error = "Appointment not found" });

            return Ok(appointment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving appointment by token");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while retrieving the appointment" });
        }
    }

    /// <summary>
    /// Cancels an appointment using the cancellation token.
    /// Public endpoint - customers can cancel their own appointments using the token.
    /// Cancellations are only allowed for future appointments (not past or already cancelled).
    /// </summary>
    /// <remarks>
    /// Soft delete: Appointments are marked as Cancelled, not physically deleted.
    /// This preserves historical data and enables audit trails.
    /// </remarks>
    /// <param name="id">The appointment ID (for route consistency)</param>
    /// <param name="token">The cancellation token provided during booking</param>
    /// <returns>Success message if cancellation completed</returns>
    [HttpPut("{id}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> CancelAppointment(
        [FromRoute] Guid id,
        [FromQuery] string token)
    {
        if (string.IsNullOrEmpty(token))
            return BadRequest(new { error = "Cancellation token is required" });

        try
        {
            var success = await _appointmentService.CancelAppointmentAsync(token);

            if (!success)
                return BadRequest(new { error = "Unable to cancel appointment. It may have already been cancelled or is in the past." });

            return Ok(new { message = "Appointment cancelled successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling appointment {AppointmentId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while cancelling the appointment" });
        }
    }

    /// <summary>
    /// Updates the status of an appointment (business owner only).
    /// Allows status transitions: Pending → Confirmed/Completed/NoShow/Cancelled
    /// </summary>
    /// <remarks>
    /// Requires authentication as a business owner.
    /// The appointment must belong to the authenticated business owner.
    /// </remarks>
    /// <param name="id">The appointment ID to update</param>
    /// <param name="request">New status information</param>
    /// <returns>Updated appointment with new status</returns>
    [Authorize]
    [HttpPut("{id}/status")]
    [ProducesResponseType(typeof(AppointmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AppointmentResponse>> UpdateAppointmentStatus(
        [FromRoute] Guid id,
        [FromBody] UpdateAppointmentStatusRequest request)
    {
        // Validate request
        var validationResult = await _updateValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return BadRequest(new { errors });
        }

        try
        {
            var businessId = GetCurrentUserId();
            var appointment = await _appointmentService.UpdateAppointmentStatusAsync(businessId, id, request.Status);

            if (appointment == null)
                return NotFound(new { error = "Appointment not found or you don't have permission to modify it" });

            return Ok(appointment);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { error = "Invalid authentication credentials" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating appointment {AppointmentId} status", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while updating the appointment" });
        }
    }
}

