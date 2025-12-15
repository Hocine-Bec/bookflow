using System.Text;
using System.Text.RegularExpressions;
using Core.Interfaces.Services;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class SlugService : ISlugService
{
    private readonly AppDbContext _context;
    
    public SlugService(AppDbContext context)
    {
        _context = context;
    }
    
    public string GenerateSlug(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;
        
        // Convert to lowercase
        var slug = input.ToLowerInvariant();
        
        // Remove accents and special characters
        slug = RemoveAccents(slug);
        
        // Replace spaces and special chars with hyphens
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"-+", "-");
        
        // Trim hyphens from start and end
        slug = slug.Trim('-');
        
        return slug;
    }
    
    public async Task<string> GenerateUniqueSlugAsync(string businessName)
    {
        var baseSlug = GenerateSlug(businessName);
        
        if (string.IsNullOrEmpty(baseSlug))
            baseSlug = "business";
        
        var slug = baseSlug;
        var counter = 1;
        
        // Check if slug exists, append number if needed
        while (await _context.Users.AnyAsync(u => u.BusinessSlug == slug))
        {
            slug = $"{baseSlug}-{counter}";
            counter++;
        }
        
        return slug;
    }
    
    private static string RemoveAccents(string text)
    {
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();
        
        foreach (var c in normalizedString)
        {
            var unicodeCategory = char.GetUnicodeCategory(c);
            if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }
        
        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }
}
