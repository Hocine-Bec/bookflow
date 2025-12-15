using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.EntityConfigurations;

public class UserConfig : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.Email).IsUnique();
        builder.HasIndex(e => e.BusinessSlug).IsUnique();

        builder.Property(e => e.Email).HasMaxLength(255).IsRequired();
        builder.Property(e => e.PasswordHash).HasMaxLength(255).IsRequired();
        builder.Property(e => e.BusinessName).HasMaxLength(255).IsRequired();
        builder.Property(e => e.BusinessSlug).HasMaxLength(255).IsRequired();
        builder.Property(e => e.BusinessPhone).HasMaxLength(20);

        builder.Property(e => e.Role)
            .HasConversion<string>()
            .HasMaxLength(50);
    }
}
