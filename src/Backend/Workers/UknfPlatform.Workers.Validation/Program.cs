using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using UknfPlatform.Application.Shared.Interfaces;
using UknfPlatform.Domain.Communication.Interfaces;
using UknfPlatform.Infrastructure.FileStorage.Services;
using UknfPlatform.Infrastructure.FileStorage.Settings;
using UknfPlatform.Infrastructure.Persistence.Contexts;
using UknfPlatform.Infrastructure.Persistence.Repositories;
using UknfPlatform.Infrastructure.Validation.Services;
using UknfPlatform.Workers.Validation.Consumers;
using UknfPlatform.Workers.Validation.Services;
using UknfPlatform.Workers.Validation.Workers;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/validation-worker-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Starting UKNF Validation Worker");

    var host = Host.CreateDefaultBuilder(args)
        .UseSerilog()
        .ConfigureServices((hostContext, services) =>
        {
            var configuration = hostContext.Configuration;

            // Database
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            // Repositories
            services.AddScoped<IReportRepository, ReportRepository>();
            services.AddScoped<IValidationResultRepository, ValidationResultRepository>();

            // File Storage
            services.Configure<FileStorageSettings>(configuration.GetSection("FileStorage"));
            services.AddScoped<IFileStorageService, LocalFileStorageService>();

            // Validation Services
            services.AddScoped<MockReportValidator>();
            services.AddScoped<ValidationResultPdfGenerator>();
            services.AddScoped<MockValidationService>();

            // Notification Services (stub for now)
            services.AddScoped<INotificationService, StubNotificationService>();

            // MassTransit with RabbitMQ
            services.AddMassTransit(x =>
            {
                // Add consumers
                x.AddConsumer<ReportValidatorConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    var rabbitMqConfig = configuration.GetSection("RabbitMQ");
                    var host = rabbitMqConfig.GetValue<string>("Host") ?? "localhost";
                    var username = rabbitMqConfig.GetValue<string>("Username") ?? "guest";
                    var password = rabbitMqConfig.GetValue<string>("Password") ?? "guest";

                    cfg.Host(host, "/", h =>
                    {
                        h.Username(username);
                        h.Password(password);
                    });

                    // Configure endpoint for report validation
                    cfg.ReceiveEndpoint("report-validation-queue", e =>
                    {
                        e.ConfigureConsumer<ReportValidatorConsumer>(context);
                        
                        // Prefetch count - process 5 messages at a time
                        e.PrefetchCount = 5;
                        
                        // Concurrency limit
                        e.UseConcurrencyLimit(5);
                        
                        // Retry policy - exponential backoff
                        e.UseMessageRetry(r => r.Exponential(
                            retryLimit: 3,
                            minInterval: TimeSpan.FromSeconds(1),
                            maxInterval: TimeSpan.FromSeconds(30),
                            intervalDelta: TimeSpan.FromSeconds(2)));
                        
                        // Message TTL - 24 hours
                        e.SetQueueArgument("x-message-ttl", 86400000);
                    });

                    cfg.ConfigureEndpoints(context);
                });
            });

            // Background workers
            services.AddHostedService<ValidationTimeoutWorker>();
        })
        .Build();

    await host.RunAsync();

    Log.Information("UKNF Validation Worker stopped gracefully");
    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "UKNF Validation Worker terminated unexpectedly");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}

