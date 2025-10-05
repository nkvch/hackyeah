namespace UknfPlatform.Application.Shared.Models.Validation;

/// <summary>
/// Final validation result with errors, warnings, and result file.
/// </summary>
public record ValidationResultResponse(
    string ValidationId,
    string Status,
    string? ResultFileUrl,
    List<ValidationError> Errors,
    List<ValidationWarning> Warnings
);

/// <summary>
/// Validation error with location and description.
/// </summary>
public record ValidationError(
    string Code,
    string Description,
    string? Location, // e.g., "Sheet1!A5" or "Row 10, Column C"
    string? SuggestedCorrection
);

/// <summary>
/// Validation warning (non-critical issue).
/// </summary>
public record ValidationWarning(
    string Code,
    string Description,
    string? Location
);


