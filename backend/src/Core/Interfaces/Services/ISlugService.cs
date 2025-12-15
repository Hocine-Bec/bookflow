namespace Core.Interfaces.Services;

public interface ISlugService
{
    Task<string> GenerateUniqueSlugAsync(string businessName);
    string GenerateSlug(string input);
}
