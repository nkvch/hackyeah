using System.Threading.RateLimiting;
using FluentValidation;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using UknfPlatform.Api.Stubs;
using UknfPlatform.Application.Auth.Authentication.Commands;
using UknfPlatform.Application.Communication.Reports.Commands;
using UknfPlatform.Application.Shared.Interfaces;
using UknfPlatform.Application.Shared.Settings;
using UknfPlatform.Domain.Auth.Interfaces;
using UknfPlatform.Domain.Communication.Interfaces;
using UknfPlatform.Domain.Shared.Interfaces;
using UknfPlatform.Infrastructure.FileStorage.Services;
using UknfPlatform.Infrastructure.FileStorage.Settings;
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

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    // Policy for resend activation endpoint - 3 requests per hour per email
    options.AddFixedWindowLimiter("resend-activation", opt =>
    {
        opt.Window = TimeSpan.FromHours(1);
        opt.PermitLimit = 3;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0; // No queuing
    });

    // Policy for set password endpoint - 5 attempts per token per hour (prevents brute force)
    options.AddFixedWindowLimiter("set-password", opt =>
    {
        opt.Window = TimeSpan.FromHours(1);
        opt.PermitLimit = 5;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0; // No queuing
    });

    // Policy for login endpoint - 5 attempts per IP per 15 minutes (prevents brute force)
    // TEMPORARILY DISABLED FOR DEVELOPMENT
    // options.AddFixedWindowLimiter("login", opt =>
    // {
    //     opt.Window = TimeSpan.FromMinutes(15);
    //     opt.PermitLimit = 5;
    //     opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    //     opt.QueueLimit = 0; // No queuing
    // });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// File Upload Configuration (for report submissions)
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104_857_600; // 100 MB
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 104_857_600; // 100 MB
    options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(5); // Timeout for large uploads
});

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Port=5432;Database=uknf_platform;Username=postgres;Password=postgres";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// MediatR
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(typeof(RegisterUserCommand).Assembly); // Auth
    config.RegisterServicesFromAssembly(typeof(SubmitReportCommand).Assembly); // Communication
    config.AddOpenBehavior(typeof(UknfPlatform.Application.Shared.Behaviors.ValidationBehavior<,>));
});

// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(RegisterUserCommandValidator).Assembly); // Auth
builder.Services.AddValidatorsFromAssembly(typeof(SubmitReportCommandValidator).Assembly); // Communication

// MassTransit (RabbitMQ for async validation) - TODO: Story 4.5
/*
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqConfig = builder.Configuration.GetSection("RabbitMQ");
        var host = rabbitMqConfig.GetValue<string>("Host") ?? "localhost";
        var username = rabbitMqConfig.GetValue<string>("Username") ?? "guest";
        var password = rabbitMqConfig.GetValue<string>("Password") ?? "guest";

        cfg.Host(host, "/", h =>
        {
            h.Username(username);
            h.Password(password);
        });

        cfg.ConfigureEndpoints(context);
    });
});
*/

// Configuration Settings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<ApplicationSettings>(builder.Configuration.GetSection("ApplicationSettings"));
builder.Services.Configure<PasswordPolicySettings>(builder.Configuration.GetSection("PasswordPolicy"));
builder.Services.Configure<FileStorageSettings>(builder.Configuration.GetSection("FileStorage"));
builder.Services.Configure<UknfPlatform.Application.Shared.Settings.JwtSettings>(builder.Configuration.GetSection("Jwt"));

// Repositories - Auth Domain
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IActivationTokenRepository, ActivationTokenRepository>();
builder.Services.AddScoped<IEmailChangeTokenRepository, EmailChangeTokenRepository>();
builder.Services.AddScoped<IPasswordHistoryRepository, PasswordHistoryRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IAuthenticationAuditLogRepository, AuthenticationAuditLogRepository>();
builder.Services.AddScoped<UknfPlatform.Domain.Auth.Repositories.IAccessRequestRepository, UknfPlatform.Infrastructure.Persistence.Repositories.AccessRequestRepository>(); // Story 2.1

// Repositories - Communication Domain
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IValidationResultRepository, ValidationResultRepository>();

// Repositories - Shared Domain (stubs for now, full implementation in Epic 2)
// TODO: Replace with proper implementation when Epic 2 is complete
builder.Services.AddScoped<IEntityRepository, StubEntityRepository>();

// Services - Auth Domain
var encryptionKey = builder.Configuration["Encryption:Key"] ?? "DefaultEncryptionKey123!@#ForDevelopment";
builder.Services.AddSingleton<IEncryptionService>(new EncryptionService(encryptionKey));
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// Services - Communication Domain
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();

// Services - Validation (Story 4.2) - COMMENTED OUT - NOT YET IMPLEMENTED
// builder.Services.AddScoped<UknfPlatform.Infrastructure.Validation.Services.MockReportValidator>();
// builder.Services.AddScoped<UknfPlatform.Infrastructure.Validation.Services.ValidationResultPdfGenerator>();
// builder.Services.AddScoped<UknfPlatform.Infrastructure.Validation.Services.MockValidationService>();
// builder.Services.AddScoped<IValidationService>(sp => 
//     sp.GetRequiredService<UknfPlatform.Infrastructure.Validation.Services.MockValidationService>());

// Services - Shared (stubs for now, full implementation in other epics)
// HTTP Context Accessor for reading JWT claims
builder.Services.AddHttpContextAccessor();

// Current User Service - reads from JWT claims in HTTP context
builder.Services.AddScoped<ICurrentUserService, UknfPlatform.Infrastructure.Identity.Services.CurrentUserService>();

// TODO: Replace with proper implementations
builder.Services.AddScoped<IEventPublisher, StubEventPublisher>();
builder.Services.AddScoped<INotificationService, StubNotificationService>();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<UknfPlatform.Application.Shared.Settings.JwtSettings>();
if (jwtSettings == null)
{
    throw new InvalidOperationException("JWT settings are not configured");
}

builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero // No grace period - enforce exact expiration
        };

        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                if (context.Exception is Microsoft.IdentityModel.Tokens.SecurityTokenExpiredException)
                {
                    context.Response.Headers.Append("Token-Expired", "true");
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

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
app.UseRateLimiter();
app.UseAuthentication(); // Must come before UseAuthorization
app.UseAuthorization();
app.MapControllers();

// Apply database migrations automatically
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        logger.LogInformation("Checking for pending database migrations...");
        var pendingMigrations = await db.Database.GetPendingMigrationsAsync();
        if (pendingMigrations.Any())
        {
            logger.LogInformation("Found {Count} pending migrations. Applying...", pendingMigrations.Count());
            foreach (var migration in pendingMigrations)
            {
                logger.LogInformation("  - {Migration}", migration);
            }
            await db.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully!");
        }
        else
        {
            logger.LogInformation("Database is up to date. No migrations needed.");
        }
        // Apply schema fixes for columns/tables added after initial migrations
        try
        {
            logger.LogInformation("Applying schema fixes...");
            // Add PendingEmail column if missing
            await db.Database.ExecuteSqlRawAsync(@"
                DO $$ BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_name = 'Users' AND column_name = 'PendingEmail'
                    ) THEN
                        ALTER TABLE ""Users"" ADD COLUMN ""PendingEmail"" character varying(256) NULL;
                    END IF;
                END $$;
            ");
            // Create EmailChangeTokens table if missing
            await db.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS ""EmailChangeTokens"" (
                    ""Id"" uuid PRIMARY KEY,
                    ""UserId"" uuid NOT NULL,
                    ""NewEmail"" character varying(256) NOT NULL,
                    ""Token"" character varying(500) NOT NULL UNIQUE,
                    ""ExpiresAt"" timestamp with time zone NOT NULL,
                    ""IsUsed"" boolean NOT NULL,
                    ""CreatedDate"" timestamp with time zone NOT NULL,
                    ""UpdatedDate"" timestamp with time zone NOT NULL,
                    FOREIGN KEY (""UserId"") REFERENCES ""Users""(""Id"") ON DELETE CASCADE
                );
                CREATE INDEX IF NOT EXISTS ""IX_EmailChangeTokens_Token"" ON ""EmailChangeTokens""(""Token"");
                CREATE INDEX IF NOT EXISTS ""IX_EmailChangeTokens_UserId_ExpiresAt"" ON ""EmailChangeTokens""(""UserId"", ""ExpiresAt"");
                CREATE INDEX IF NOT EXISTS ""IX_EmailChangeTokens_IsUsed"" ON ""EmailChangeTokens""(""IsUsed"");
            ");
            
            // Create AccessRequests table if missing
            await db.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS ""AccessRequests"" (
                    ""Id"" uuid PRIMARY KEY,
                    ""UserId"" uuid NOT NULL,
                    ""Status"" character varying(20) NOT NULL,
                    ""SubmittedDate"" timestamp with time zone NULL,
                    ""ReviewedByUserId"" uuid NULL,
                    ""ReviewedDate"" timestamp with time zone NULL,
                    ""CreatedDate"" timestamp with time zone NOT NULL,
                    ""UpdatedDate"" timestamp with time zone NOT NULL,
                    FOREIGN KEY (""UserId"") REFERENCES ""Users""(""Id"") ON DELETE RESTRICT,
                    FOREIGN KEY (""ReviewedByUserId"") REFERENCES ""Users""(""Id"") ON DELETE RESTRICT
                );
                CREATE INDEX IF NOT EXISTS ""IX_AccessRequests_Status"" ON ""AccessRequests""(""Status"");
                CREATE INDEX IF NOT EXISTS ""IX_AccessRequests_UserId"" ON ""AccessRequests""(""UserId"");
                CREATE INDEX IF NOT EXISTS ""IX_AccessRequests_UserId_Status"" ON ""AccessRequests""(""UserId"", ""Status"");
                CREATE INDEX IF NOT EXISTS ""IX_AccessRequests_ReviewedByUserId"" ON ""AccessRequests""(""ReviewedByUserId"");
            ");
            
            logger.LogInformation("Schema fixes applied successfully!");
        }
        catch (Exception schemaEx)
        {
            logger.LogWarning(schemaEx, "Schema fix failed. This may be normal if using non-PostgreSQL database.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while applying database migrations. Application will continue, but database may be in inconsistent state.");
        // In development, we want to see this error clearly
        if (app.Environment.IsDevelopment())
        {
            throw;
        }
    }
}

app.Run();

// Make Program accessible to integration tests
public partial class Program { }
