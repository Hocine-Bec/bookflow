using System.Security.Claims;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.DTOs.Common;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BusinessHoursController(IBusinessHoursService businessHoursService) : ControllerBase
{
    // GET: api/businesshours/my-hours
    [HttpGet("my-hours")]
    [Authorize]
    public async Task<IActionResult> GetMyHours()
    {
        var userId = GetCurrentUserId();
        var hours = await businessHoursService.GetBusinessHoursByBusinessIdAsync(userId);
        
        return Ok(hours);
    }
    
    // POST: api/businesshours/set-hours
    [HttpPost("set-hours")]
    [Authorize]
    public async Task<IActionResult> SetHours([FromBody] dynamic request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ErrorResponse("Validation failed", GetModelStateErrors()));
        }
        
        var userId = GetCurrentUserId();
        
        try
        {
            var hours = await businessHoursService.SetBusinessHoursAsync(userId, request);
            return Ok(hours);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorResponse(ex.Message));
        }
    }
    
    // Helper methods
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }
    
    private Dictionary<string, string[]> GetModelStateErrors()
    {
        return ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
            );
    }
}
