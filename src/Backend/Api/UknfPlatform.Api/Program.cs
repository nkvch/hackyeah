using FluentValidation;
using Microsoft.EntityFrameworkCore;
using UknfPlatform.Application.Auth.Authentication.Commands;
using UknfPlatform.Application.Shared.Interfaces;
using UknfPlatform.Application.Shared.Settings;
using UknfPlatform.Domain.Auth.Interfaces;
using UknfPlatform.Infrastructure.Identity.Services;
using UknfPlatform.Infrastructure.Persistence.Contexts;
using UknfPlatform.Infrastructure.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "UKNF Communication Platform API",
        Version = "v1",
        Description = "REST API for UKNF Communication Platform"
    });
});

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Port=5432;Database=uknf_platform;Username=postgres;Password=postgres";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// MediatR
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(typeof(RegisterUserCommand).Assembly);
    config.AddOpenBehavior(typeof(UknfPlatform.Application.Shared.Behaviors.ValidationBehavior<,>));
});

// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(RegisterUserCommandValidator).Assembly);

// Configuration Settings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<ApplicationSettings>(builder.Configuration.GetSection("ApplicationSettings"));

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IActivationTokenRepository, ActivationTokenRepository>();

// Services
var encryptionKey = builder.Configuration["Encryption:Key"] ?? "DefaultEncryptionKey123!@#ForDevelopment";
builder.Services.AddSingleton<IEncryptionService>(new EncryptionService(encryptionKey));
builder.Services.AddScoped<IEmailService, EmailService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

// Ensure database is created (for development)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (app.Environment.IsDevelopment())
    {
        await db.Database.EnsureCreatedAsync();
    }
}

app.Run();

// Make Program accessible to integration tests
public partial class Program { }
