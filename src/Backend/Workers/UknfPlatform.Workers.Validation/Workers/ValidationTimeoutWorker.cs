using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UknfPlatform.Domain.Communication.Entities;
using UknfPlatform.Domain.Communication.Enums;
using UknfPlatform.Domain.Communication.Interfaces;
using UknfPlatform.Infrastructure.Persistence.Contexts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace UknfPlatform.Workers.Validation.Workers;

/// <summary>
/// Background worker that checks for validation timeouts (>24 hours) and updates report status.
/// </summary>
public class ValidationTimeoutWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ValidationTimeoutWorker> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1); // Check every hour
    private readonly TimeSpan _timeoutThreshold = TimeSpan.FromHours(24);

    public ValidationTimeoutWorker(
        IServiceProvider serviceProvider,
        ILogger<ValidationTimeoutWorker> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ValidationTimeoutWorker started. Checking every {Interval} minutes",
            _checkInterval.TotalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckForTimeoutsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during timeout check");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("ValidationTimeoutWorker stopped");
    }

    private async Task CheckForTimeoutsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var reportRepository = scope.ServiceProvider.GetRequiredService<IReportRepository>();

        var cutoffTime = DateTime.UtcNow - _timeoutThreshold;

        _logger.LogDebug("Checking for reports with ValidationStartedDate < {CutoffTime}", cutoffTime);

        // Find reports that have been in "Ongoing" or "Transmitted" status for more than 24 hours
        var timedOutReports = await context.Reports
            .Where(r =>
                (r.ValidationStatus == ValidationStatus.Ongoing || r.ValidationStatus == ValidationStatus.Transmitted) &&
                r.ValidationStartedDate.HasValue &&
                r.ValidationStartedDate.Value < cutoffTime)
            .ToListAsync(cancellationToken);

        if (!timedOutReports.Any())
        {
            _logger.LogDebug("No timed-out reports found");
            return;
        }

        _logger.LogWarning("Found {Count} timed-out reports", timedOutReports.Count);

        foreach (var report in timedOutReports)
        {
            try
            {
                _logger.LogWarning(
                    "Marking report {ReportId} as TimeoutError. Started: {StartedDate}, Duration: {Duration} hours",
                    report.Id,
                    report.ValidationStartedDate,
                    (DateTime.UtcNow - report.ValidationStartedDate!.Value).TotalHours);

                report.RecordTimeoutError();
                await reportRepository.UpdateAsync(report, cancellationToken);

                // Send timeout notification
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                await notificationService.SendValidationTimeoutNotificationAsync(
                    report.UserId,
                    report.Id,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to mark report {ReportId} as timed out", report.Id);
            }
        }

        await reportRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully marked {Count} reports as TimeoutError", timedOutReports.Count);
    }
}

