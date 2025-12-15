using Core.Entities;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace Infrastructure.Data.Seed;

public static class Seeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Users.AnyAsync()) return;

        // Create a sample business user
        var businessUser = new User
        {
            Email = "owner@bookflow.test",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("TestPass123!"), // Properly hashed password
            Role = Core.Enums.UserRole.Business,
            BusinessName = "Test Business",
            BusinessSlug = "test-business",
            BusinessPhone = "+10000000000",
            IsEmailVerified = true
        };

        // Add services
        var service = new Service
        {
            Business = businessUser,
            Name = "General Consultation",
            Description = "A standard 30-minute consultation.",
            DurationMinutes = 30,
            Price = 50.00m,
            IsActive = true
        };

        // Add business hours
        var hours = new BusinessHours
        {
            Business = businessUser,
            DayOfWeek = DayOfWeek.Monday,
            OpenTime = new TimeOnly(9, 0),
            CloseTime = new TimeOnly(17, 0),
            IsOpen = true
        };

        // Add sample appointment
        var appointment = new Appointment
        {
            Business = businessUser,
            Service = service,
            CustomerName = "John Doe",
            CustomerEmail = "johndoe@example.com",
            CustomerPhone = "+19999999999",
            AppointmentDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(10, 30),
            Status = Core.Enums.AppointmentStatus.Pending,
            CancellationToken = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20)
        };

        db.Users.Add(businessUser);
        db.Services.Add(service);
        db.BusinessHours.Add(hours);
        db.Appointments.Add(appointment);

        await db.SaveChangesAsync();
    }
}

