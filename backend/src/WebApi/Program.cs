using System.Text;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Core.Services;
using FluentValidation;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using DotNetEnv;
using WebApi.Mappings;
using WebApi.Validators;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;

Env.Load(); 

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger configuration with JWT support
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Bookflow System", 
        Version = "v1",
        Description = "",
    });
    
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your token"
    });
    
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Database configuration from environment variables
var connectionString = Environment.GetEnvironmentVariable("DEFAULT_CONNECTION") ?? string.Empty;
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

// JWT Authentication configuration
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? "";
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "BookingSystem.API";
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "BookingSystem.Client";
var lifetime = int.Parse(Environment.GetEnvironmentVariable("JWT_EXPIRY_MINUTES") ?? "15");

if (string.IsNullOrEmpty(jwtSecret) || jwtSecret.Length < 32)
{
    throw new InvalidOperationException("JWT_SECRET_KEY must be set in environment and be at least 32 characters long.");
}

var secretKey = Encoding.UTF8.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ClockSkew = TimeSpan.Zero // Remove default 5 minute tolerance
    };
});

builder.Services.AddAuthorization();

// Register Mapster mapping profiles with dependency injection
var config = TypeAdapterConfig.GlobalSettings;
config.Scan(typeof(AppointmentMappings).Assembly);
builder.Services.AddMapster();

// Register FluentValidation validators
builder.Services.AddValidatorsFromAssemblyContaining<CreateAppointmentRequestValidator>(ServiceLifetime.Transient);

// Register Repositories (Infrastructure - Data Access)
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
builder.Services.AddScoped<IBusinessHoursRepository, BusinessHoursRepository>();

// Register Services (Core - Business Logic)
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISlugService, SlugService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IBusinessService, BusinessService>();
builder.Services.AddScoped<IServiceManagementService, ServiceManagementService>();
builder.Services.AddScoped<IBusinessHoursService, BusinessHoursService>();

// CORS configuration
var corsOrigins = Environment.GetEnvironmentVariable("CORS_ORIGINS") ?? "http://localhost:5173,http://localhost:3000";
var origins = corsOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(origins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Apply migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    await Infrastructure.Data.Seed.Seeder.SeedAsync(db);
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("Bookflow")
            .WithTheme(ScalarTheme.Kepler)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
            .WithOpenApiRoutePattern("/swagger/{documentName}/swagger.json"); 
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
