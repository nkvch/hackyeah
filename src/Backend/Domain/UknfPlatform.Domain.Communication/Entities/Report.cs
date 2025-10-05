using UknfPlatform.Domain.Communication.Enums;
using UknfPlatform.Domain.Shared.Common;

namespace UknfPlatform.Domain.Communication.Entities;

/// <summary>
/// Report entity representing regulatory reports submitted by supervised entities
/// </summary>
public class Report : BaseEntity
{
    // Entity and User references
    public long EntityId { get; private set; }
    public Guid UserId { get; private set; }
    
    // File information
    public string FileName { get; private set; } = string.Empty;
    public string FileStorageKey { get; private set; } = string.Empty;
    public long FileSize { get; private set; }
    
    // Report metadata
    public string ReportType { get; private set; } = string.Empty;
    public string ReportingPeriod { get; private set; } = string.Empty;
    
    // Validation information
    public ValidationStatus ValidationStatus { get; private set; }
    public string? ValidationResultFileKey { get; private set; }
    public string? UniqueValidationId { get; private set; }
    
    // Archival and correction
    public bool IsArchived { get; private set; }
    public Guid? IsCorrectionOfReportId { get; private set; }
    
    // Timestamps
    public DateTime SubmittedDate { get; private set; }
    public DateTime? ValidationStartedDate { get; private set; }
    public DateTime? ValidationCompletedDate { get; private set; }
    
    // Error and contestation information
    public string? ErrorDescription { get; private set; }
    public string? ContestedDescription { get; private set; }
    public Guid? ContestedByUserId { get; private set; }
    public DateTime? ContestedDate { get; private set; }

    // EF Core requires parameterless constructor
    private Report() { }

    /// <summary>
    /// Creates a new report after successful file upload
    /// </summary>
    public static Report Create(
        long entityId,
        Guid userId,
        string fileName,
        string fileStorageKey,
        long fileSize,
        string reportType,
        string reportingPeriod)
    {
        ValidateCreateData(entityId, userId, fileName, fileStorageKey, fileSize, reportType, reportingPeriod);

        return new Report
        {
            EntityId = entityId,
            UserId = userId,
            FileName = fileName,
            FileStorageKey = fileStorageKey,
            FileSize = fileSize,
            ReportType = reportType,
            ReportingPeriod = reportingPeriod,
            ValidationStatus = ValidationStatus.Working,
            IsArchived = false,
            SubmittedDate = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a correction report linked to original
    /// </summary>
    public static Report CreateCorrection(
        long entityId,
        Guid userId,
        string fileName,
        string fileStorageKey,
        long fileSize,
        string reportType,
        string reportingPeriod,
        Guid originalReportId)
    {
        ValidateCreateData(entityId, userId, fileName, fileStorageKey, fileSize, reportType, reportingPeriod);

        if (originalReportId == Guid.Empty)
            throw new ArgumentException("Original report ID must be valid", nameof(originalReportId));

        return new Report
        {
            EntityId = entityId,
            UserId = userId,
            FileName = fileName,
            FileStorageKey = fileStorageKey,
            FileSize = fileSize,
            ReportType = reportType,
            ReportingPeriod = reportingPeriod,
            ValidationStatus = ValidationStatus.Working,
            IsArchived = false,
            SubmittedDate = DateTime.UtcNow,
            IsCorrectionOfReportId = originalReportId
        };
    }

    /// <summary>
    /// Updates validation status to Transmitted and records validation ID
    /// </summary>
    public void StartValidation(string uniqueValidationId)
    {
        if (ValidationStatus != ValidationStatus.Working)
            throw new InvalidOperationException($"Cannot start validation. Current status: {ValidationStatus}");

        if (string.IsNullOrWhiteSpace(uniqueValidationId))
            throw new ArgumentException("Validation ID is required", nameof(uniqueValidationId));

        ValidationStatus = ValidationStatus.Transmitted;
        UniqueValidationId = uniqueValidationId;
        ValidationStartedDate = DateTime.UtcNow;
        UpdateTimestamp();
    }

    /// <summary>
    /// Updates validation status to Ongoing
    /// </summary>
    public void UpdateToOngoing()
    {
        if (ValidationStatus != ValidationStatus.Transmitted)
            throw new InvalidOperationException($"Cannot update to Ongoing. Current status: {ValidationStatus}");

        ValidationStatus = ValidationStatus.Ongoing;
        UpdateTimestamp();
    }

    /// <summary>
    /// Completes validation successfully
    /// </summary>
    public void CompleteValidationSuccessfully(string? validationResultFileKey = null)
    {
        if (ValidationStatus != ValidationStatus.Ongoing && ValidationStatus != ValidationStatus.Transmitted)
            throw new InvalidOperationException($"Cannot complete validation. Current status: {ValidationStatus}");

        ValidationStatus = ValidationStatus.Successful;
        ValidationResultFileKey = validationResultFileKey;
        ValidationCompletedDate = DateTime.UtcNow;
        UpdateTimestamp();
    }

    /// <summary>
    /// Completes validation with errors
    /// </summary>
    public void CompleteValidationWithErrors(string errorDescription, string? validationResultFileKey = null)
    {
        if (ValidationStatus != ValidationStatus.Ongoing && ValidationStatus != ValidationStatus.Transmitted)
            throw new InvalidOperationException($"Cannot complete validation. Current status: {ValidationStatus}");

        if (string.IsNullOrWhiteSpace(errorDescription))
            throw new ArgumentException("Error description is required", nameof(errorDescription));

        ValidationStatus = ValidationStatus.ValidationErrors;
        ErrorDescription = errorDescription;
        ValidationResultFileKey = validationResultFileKey;
        ValidationCompletedDate = DateTime.UtcNow;
        UpdateTimestamp();
    }

    /// <summary>
    /// Records technical error during validation
    /// </summary>
    public void RecordTechnicalError(string errorDescription)
    {
        if (string.IsNullOrWhiteSpace(errorDescription))
            throw new ArgumentException("Error description is required", nameof(errorDescription));

        ValidationStatus = ValidationStatus.TechnicalError;
        ErrorDescription = errorDescription;
        ValidationCompletedDate = DateTime.UtcNow;
        UpdateTimestamp();
    }

    /// <summary>
    /// Records timeout error (24-hour validation exceeded)
    /// </summary>
    public void RecordTimeoutError()
    {
        ValidationStatus = ValidationStatus.TimeoutError;
        ErrorDescription = "Validation process exceeded 24-hour timeout";
        ValidationCompletedDate = DateTime.UtcNow;
        UpdateTimestamp();
    }

    /// <summary>
    /// Contests the report by UKNF employee
    /// </summary>
    public void ContestByUKNF(Guid uknfUserId, string contestedDescription)
    {
        if (uknfUserId == Guid.Empty)
            throw new ArgumentException("UKNF user ID must be valid", nameof(uknfUserId));

        if (string.IsNullOrWhiteSpace(contestedDescription))
            throw new ArgumentException("Contestation description is required", nameof(contestedDescription));

        ValidationStatus = ValidationStatus.ContestedByUKNF;
        ContestedByUserId = uknfUserId;
        ContestedDescription = contestedDescription;
        ContestedDate = DateTime.UtcNow;
        UpdateTimestamp();
    }

    /// <summary>
    /// Archives the report
    /// </summary>
    public void Archive()
    {
        IsArchived = true;
        UpdateTimestamp();
    }

    /// <summary>
    /// Unarchives the report
    /// </summary>
    public void Unarchive()
    {
        IsArchived = false;
        UpdateTimestamp();
    }

    private static void ValidateCreateData(
        long entityId,
        Guid userId,
        string fileName,
        string fileStorageKey,
        long fileSize,
        string reportType,
        string reportingPeriod)
    {
        if (entityId <= 0)
            throw new ArgumentException("Entity ID must be positive", nameof(entityId));

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID must be valid", nameof(userId));

        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name is required", nameof(fileName));

        if (string.IsNullOrWhiteSpace(fileStorageKey))
            throw new ArgumentException("File storage key is required", nameof(fileStorageKey));

        if (fileSize <= 0)
            throw new ArgumentException("File size must be positive", nameof(fileSize));

        if (string.IsNullOrWhiteSpace(reportType))
            throw new ArgumentException("Report type is required", nameof(reportType));

        if (string.IsNullOrWhiteSpace(reportingPeriod))
            throw new ArgumentException("Reporting period is required", nameof(reportingPeriod));
    }
}

