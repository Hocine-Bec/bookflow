using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.EntityConfigurations;

public class AppointmentConfig : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => new { e.BusinessId, e.AppointmentDate });
        builder.HasIndex(e => e.CustomerEmail);
        builder.HasIndex(e => e.CancellationToken).IsUnique();

        builder.Property(e => e.CustomerName).HasMaxLength(255).IsRequired();
        builder.Property(e => e.CustomerEmail).HasMaxLength(255).IsRequired();
        builder.Property(e => e.CustomerPhone).HasMaxLength(20).IsRequired();

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasOne(e => e.Service)
            .WithMany(s => s.Appointments)
            .HasForeignKey(e => e.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Business)
            .WithMany(u => u.Appointments)
            .HasForeignKey(e => e.BusinessId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
