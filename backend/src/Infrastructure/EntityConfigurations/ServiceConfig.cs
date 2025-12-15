using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.EntityConfigurations;

public class ServiceConfig : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => new { e.BusinessId, e.IsActive });

        builder.Property(e => e.Name).HasMaxLength(255).IsRequired();
        builder.Property(e => e.Price).HasPrecision(10, 2);

        builder.HasOne(e => e.Business)
            .WithMany(u => u.Services)
            .HasForeignKey(e => e.BusinessId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
