namespace UknfPlatform.Application.Shared.Models.Validation;

/// <summary>
/// Current validation status and progress information.
/// </summary>
public record ValidationStatusResponse(
    string ValidationId,
    string Status,
    int Progress, // 0-100 percentage
    DateTime? EstimatedCompletion
);


