using MassTransit;
using Microsoft.Extensions.Logging;
using UknfPlatform.Application.Shared.Interfaces;
using UknfPlatform.Domain.Communication.Entities;
using UknfPlatform.Domain.Communication.Interfaces;
using UknfPlatform.Domain.Communication.Messages;
using UknfPlatform.Infrastructure.Validation.Services;
using Polly;
using Polly.Retry;

namespace UknfPlatform.Workers.Validation.Consumers;

/// <summary>
/// MassTransit consumer that processes report validation jobs from RabbitMQ.
/// </summary>
public class ReportValidatorConsumer : IConsumer<ReportValidationJob>
{
    private readonly IReportRepository _reportRepository;
    private readonly IValidationResultRepository _validationResultRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly MockValidationService _validationService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<ReportValidatorConsumer> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;

    public ReportValidatorConsumer(
        IReportRepository reportRepository,
        IValidationResultRepository validationResultRepository,
        IFileStorageService fileStorageService,
        MockValidationService validationService,
        INotificationService notificationService,
        ILogger<ReportValidatorConsumer> logger)
    {
        _reportRepository = reportRepository ?? throw new ArgumentNullException(nameof(reportRepository));
        _validationResultRepository = validationResultRepository ?? throw new ArgumentNullException(nameof(validationResultRepository));
        _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Configure retry policy with exponential backoff
        _retryPolicy = Policy
            .Handle<Exception>(ex => !(ex is InvalidOperationException)) // Don't retry business logic errors
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(exception,
                        "Retry {RetryCount} for report {ReportId} after {Delay}s",
                        retryCount, context["ReportId"], timeSpan.TotalSeconds);
                });
    }

    public async Task Consume(ConsumeContext<ReportValidationJob> context)
    {
        var job = context.Message;
        var correlationId = context.CorrelationId?.ToString() ?? Guid.NewGuid().ToString();

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["ReportId"] = job.ReportId,
            ["EntityId"] = job.EntityId
        }))
        {
            _logger.LogInformation(
                "Processing validation job for report {ReportId}, file: {FileName}",
                job.ReportId, job.FileName);

            try
            {
                await _retryPolicy.ExecuteAsync(async ctx =>
                {
                    await ProcessValidationJobAsync(job, context.CancellationToken);
                }, new Dictionary<string, object> { ["ReportId"] = job.ReportId });

                _logger.LogInformation("Validation job completed successfully for report {ReportId}", job.ReportId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error processing validation job for report {ReportId}", job.ReportId);
                await HandleFatalErrorAsync(job.ReportId, ex.Message, context.CancellationToken);
                
                // Don't throw - we've handled the error by updating the report status
                // Throwing would cause MassTransit to retry indefinitely
            }
        }
    }

    private async Task ProcessValidationJobAsync(ReportValidationJob job, CancellationToken cancellationToken)
    {
        // 1. Fetch report from database
        var report = await _reportRepository.GetByIdAsync(job.ReportId, cancellationToken);
        if (report == null)
        {
            _logger.LogError("Report {ReportId} not found in database", job.ReportId);
            throw new InvalidOperationException($"Report {job.ReportId} not found");
        }

        // 2. Update report status to Ongoing
        report.UpdateToOngoing();
        await _reportRepository.UpdateAsync(report, cancellationToken);
        await _reportRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Report {ReportId} status updated to Ongoing", job.ReportId);

        // 3. Create ValidationResult entity
        var validationResult = ValidationResult.Create(job.ReportId, report.UniqueValidationId!);
        await _validationResultRepository.AddAsync(validationResult, cancellationToken);
        await _validationResultRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("ValidationResult entity created for report {ReportId}", job.ReportId);

        // 4. Download file from storage
        Stream fileStream;
        try
        {
            fileStream = await _fileStorageService.DownloadFileAsync(job.FileStorageKey, cancellationToken);
            _logger.LogInformation("File downloaded from storage for report {ReportId}", job.ReportId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download file from storage for report {ReportId}", job.ReportId);
            throw new InvalidOperationException("Failed to download report file from storage", ex);
        }

        // 5. Process validation using MockValidationService
        ValidationProcessingResult processingResult;
        try
        {
            processingResult = await _validationService.ProcessValidationAsync(
                report.UniqueValidationId!,
                cancellationToken);

            _logger.LogInformation("Validation processing completed for report {ReportId}. IsValid: {IsValid}",
                job.ReportId, processingResult.IsValid);
        }
        finally
        {
            await fileStream.DisposeAsync();
        }

        // 6. Update ValidationResult entity
        if (processingResult.IsValid)
        {
            validationResult.CompleteValidation(
                isValid: true,
                errorsJson: null,
                warningsJson: processingResult.WarningsJson,
                resultFileStorageKey: processingResult.ResultFileStorageKey,
                extractedMetadataJson: processingResult.ExtractedMetadataJson);

            report.CompleteValidationSuccessfully(processingResult.ResultFileStorageKey);
        }
        else
        {
            validationResult.CompleteValidation(
                isValid: false,
                errorsJson: processingResult.ErrorsJson,
                warningsJson: processingResult.WarningsJson,
                resultFileStorageKey: processingResult.ResultFileStorageKey,
                extractedMetadataJson: processingResult.ExtractedMetadataJson);

            var errorSummary = $"Validation found {processingResult.ErrorsJson?.Length ?? 0} errors";
            report.CompleteValidationWithErrors(errorSummary, processingResult.ResultFileStorageKey);
        }

        // 7. Save final state
        await _validationResultRepository.UpdateAsync(validationResult, cancellationToken);
        await _reportRepository.UpdateAsync(report, cancellationToken);

        await _validationResultRepository.SaveChangesAsync(cancellationToken);
        await _reportRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Report {ReportId} final status: {Status}", job.ReportId, report.ValidationStatus);

        // 8. Send notifications
        var statusMessage = processingResult.IsValid
            ? "Your report has been validated successfully."
            : "Your report validation completed with errors. Please review the validation result.";

        await _notificationService.SendReportStatusUpdateAsync(
            job.UserId,
            job.ReportId,
            report.ValidationStatus.ToString(),
            statusMessage,
            cancellationToken);

        // TODO: Send email notification with validation result PDF attached (Task 14)
        // await _emailService.SendValidationResultEmailAsync(...);
    }

    private async Task HandleFatalErrorAsync(Guid reportId, string errorMessage, CancellationToken cancellationToken)
    {
        try
        {
            var report = await _reportRepository.GetByIdAsync(reportId, cancellationToken);
            if (report != null)
            {
                report.RecordTechnicalError($"Validation failed: {errorMessage}");
                await _reportRepository.UpdateAsync(report, cancellationToken);
                await _reportRepository.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Report {ReportId} marked as TechnicalError", reportId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update report {ReportId} with technical error", reportId);
            // Can't do much more here - log and continue
        }
    }
}

