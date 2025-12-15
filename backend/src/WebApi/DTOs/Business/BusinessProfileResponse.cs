using WebApi.DTOs.Services;

namespace WebApi.DTOs.Business;

public class BusinessProfileResponse
{
    public Guid Id { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public string BusinessSlug { get; set; } = string.Empty;
    public string? BusinessPhone { get; set; }
    public string? BusinessDescription { get; set; }
    public List<ServiceResponse> Services { get; set; } = new();
}
