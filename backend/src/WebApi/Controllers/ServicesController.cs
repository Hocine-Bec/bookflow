using System.Security.Claims;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.DTOs.Common;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServicesController(IServiceManagementService serviceManagementService) : ControllerBase
{
    // GET: api/services/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var service = await serviceManagementService.GetServiceByIdAsync(id);
        
        if (service == null)
        {
            return NotFound(new ErrorResponse("Service not found"));
        }
        
        return Ok(service);
    }
    
    // GET: api/services/my-services
    [HttpGet("my-services")]
    [Authorize]
    public async Task<IActionResult> GetMyServices()
    {
        var userId = GetCurrentUserId();
        var services = await serviceManagementService.GetServicesByBusinessIdAsync(userId);
        
        return Ok(services);
    }
    
    // POST: api/services
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] dynamic request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ErrorResponse("Validation failed", GetModelStateErrors()));
        }
        
        var userId = GetCurrentUserId();
        var service = await serviceManagementService.CreateServiceAsync(userId, request);
        
        return CreatedAtAction(nameof(GetById), new { id = service.Id }, service);
    }
    
    // PUT: api/services/{id}
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] dynamic request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ErrorResponse("Validation failed", GetModelStateErrors()));
        }
        
        var userId = GetCurrentUserId();
        var service = await serviceManagementService.UpdateServiceAsync(userId, id, request);
        
        if (service == null)
        {
            return NotFound(new ErrorResponse("Service not found or you don't have permission to update it"));
        }
        
        return Ok(service);
    }
    
    // DELETE: api/services/{id}
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetCurrentUserId();
        var success = await serviceManagementService.DeleteServiceAsync(userId, id);
        
        if (!success)
        {
            return NotFound(new ErrorResponse("Service not found or you don't have permission to delete it"));
        }
        
        return NoContent();
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
