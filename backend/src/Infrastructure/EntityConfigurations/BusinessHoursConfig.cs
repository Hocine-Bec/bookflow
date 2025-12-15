using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.EntityConfigurations;

public class BusinessHoursConfig : IEntityTypeConfiguration<BusinessHours>
{
    public void Configure(EntityTypeBuilder<BusinessHours> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => new { e.BusinessId, e.DayOfWeek }).IsUnique();

        builder.Property(e => e.DayOfWeek).HasConversion<int>();

        builder.HasOne(e => e.Business)
            .WithMany(u => u.BusinessHours)
            .HasForeignKey(e => e.BusinessId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
