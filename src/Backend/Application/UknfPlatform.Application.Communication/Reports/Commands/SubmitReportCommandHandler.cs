using MediatR;
using Microsoft.Extensions.Logging;
using UknfPlatform.Application.Shared.Interfaces;
using UknfPlatform.Domain.Communication.Entities;
using UknfPlatform.Domain.Communication.Events;
using UknfPlatform.Domain.Communication.Interfaces;
using UknfPlatform.Domain.Shared.Interfaces;

namespace UknfPlatform.Application.Communication.Reports.Commands;

/// <summary>
/// Handler for SubmitReportCommand
/// Uploads file, creates report entity, and publishes event for validation
/// </summary>
public class SubmitReportCommandHandler : IRequestHandler<SubmitReportCommand, SubmitReportResponse>
{
    private readonly IReportRepository _reportRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEntityRepository _entityRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<SubmitReportCommandHandler> _logger;

    public SubmitReportCommandHandler(
        IReportRepository reportRepository,
        IFileStorageService fileStorageService,
        ICurrentUserService currentUserService,
        IEntityRepository entityRepository,
        IEventPublisher eventPublisher,
        ILogger<SubmitReportCommandHandler> logger)
    {
        _reportRepository = reportRepository ?? throw new ArgumentNullException(nameof(reportRepository));
        _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _entityRepository = entityRepository ?? throw new ArgumentNullException(nameof(entityRepository));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SubmitReportResponse> Handle(SubmitReportCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing report submission for entity {EntityId}, type {ReportType}, period {ReportingPeriod}",
            request.EntityId, request.ReportType, request.ReportingPeriod);

        // 1. Check user permission (authorization)
        var hasPermission = await _currentUserService.HasPermissionAsync(
            "communication.reports.submit", 
            request.EntityId, 
            cancellationToken);

        if (!hasPermission)
        {
            _logger.LogWarning(
                "User {UserId} lacks permission to submit reports for entity {EntityId}",
                _currentUserService.UserId, request.EntityId);
            throw new UnauthorizedAccessException($"You don't have permission to submit reports for this entity");
        }

        // 2. Verify entity exists
        var entity = await _entityRepository.GetByIdAsync(request.EntityId, cancellationToken);
        if (entity == null)
        {
            _logger.LogWarning("Entity {EntityId} not found", request.EntityId);
            throw new InvalidOperationException($"Entity with ID {request.EntityId} not found");
        }

        // 3. Check for duplicate report (same entity, type, period)
        var isDuplicate = await _reportRepository.ExistsAsync(
            request.EntityId,
            request.ReportType,
            request.ReportingPeriod,
            cancellationToken);

        if (isDuplicate)
        {
            _logger.LogWarning(
                "Duplicate report submission attempt for entity {EntityId}, type {ReportType}, period {ReportingPeriod}",
                request.EntityId, request.ReportType, request.ReportingPeriod);
            throw new InvalidOperationException(
                $"A report for {request.ReportType} - {request.ReportingPeriod} already exists for this entity. " +
                "Please archive or delete the existing report before submitting a new one.");
        }

        // 4. Upload file to storage
        string fileStorageKey;
        try
        {
            using var fileStream = request.File!.OpenReadStream();
            fileStorageKey = await _fileStorageService.UploadFileAsync(
                fileStream,
                request.File.FileName,
                request.File.ContentType,
                cancellationToken);

            _logger.LogInformation(
                "File uploaded successfully. Storage key: {StorageKey}, Size: {Size} bytes",
                fileStorageKey, request.File.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload file: {FileName}", request.File?.FileName);
            throw new InvalidOperationException("Failed to upload file to storage. Please try again.", ex);
        }

        // 5. Create report domain entity
        var report = Report.Create(
            request.EntityId,
            _currentUserService.UserId,
            request.File!.FileName,
            fileStorageKey,
            request.File.Length,
            request.ReportType,
            request.ReportingPeriod);

        // 6. Save report to database
        try
        {
            await _reportRepository.AddAsync(report, cancellationToken);
            // Note: SaveChanges will be called by Unit of Work in API layer
            
            _logger.LogInformation(
                "Report created successfully. ReportId: {ReportId}, Entity: {EntityId}, User: {UserId}",
                report.Id, request.EntityId, _currentUserService.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save report to database");
            
            // Rollback: Delete uploaded file
            try
            {
                await _fileStorageService.DeleteFileAsync(fileStorageKey, cancellationToken);
                _logger.LogInformation("Rolled back file upload: {StorageKey}", fileStorageKey);
            }
            catch (Exception deleteEx)
            {
                _logger.LogError(deleteEx, "Failed to rollback file upload: {StorageKey}", fileStorageKey);
            }

            throw new InvalidOperationException("Failed to save report. Please try again.", ex);
        }

        // 7. Publish ReportSubmittedEvent (triggers validation in Story 4.2)
        try
        {
            var reportSubmittedEvent = new ReportSubmittedEvent
            {
                ReportId = report.Id,
                EntityId = request.EntityId,
                UserId = _currentUserService.UserId,
                FileName = request.File.FileName,
                FileStorageKey = fileStorageKey,
                ReportType = request.ReportType,
                ReportingPeriod = request.ReportingPeriod,
                SubmittedDate = report.SubmittedDate
            };

            await _eventPublisher.PublishAsync(reportSubmittedEvent, cancellationToken);
            
            _logger.LogInformation(
                "ReportSubmittedEvent published for report {ReportId}",
                report.Id);
        }
        catch (Exception ex)
        {
            // Don't fail the operation if event publishing fails
            // The report is saved, validation can be triggered manually if needed
            _logger.LogError(ex, "Failed to publish ReportSubmittedEvent for report {ReportId}", report.Id);
        }

        // 8. Return response
        return new SubmitReportResponse
        {
            ReportId = report.Id,
            UniqueValidationId = null, // Will be set after transmission to validation service
            Status = report.ValidationStatus.ToString(),
            Message = "Report submitted successfully and queued for validation",
            SubmittedDate = report.SubmittedDate,
            SubmitterName = _currentUserService.FullName,
            EntityName = entity.Name
        };
    }
}

