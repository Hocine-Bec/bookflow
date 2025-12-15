using Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using WebApi.DTOs.Common;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BusinessController(IBusinessService businessService) : ControllerBase
{
    // GET: api/business/by-slug/{slug}
    [HttpGet("by-slug/{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var business = await businessService.GetBusinessBySlugAsync(slug);
        
        if (business == null)
        {
            return NotFound(new ErrorResponse("Business not found"));
        }
        
        return Ok(business);
    }
    
    // GET: api/business/{businessId}/hours
    [HttpGet("{businessId}/hours")]
    public async Task<IActionResult> GetBusinessHours(Guid businessId)
    {
        var hours = await businessService.GetBusinessHoursAsync(businessId);
        
        if (hours == null)
        {
            return NotFound(new ErrorResponse("Business not found"));
        }
        
        return Ok(hours);
    }
}
