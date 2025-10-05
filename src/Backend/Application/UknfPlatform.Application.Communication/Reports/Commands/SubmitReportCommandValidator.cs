using FluentValidation;
using UknfPlatform.Application.Communication.Common;

namespace UknfPlatform.Application.Communication.Reports.Commands;

/// <summary>
/// Validator for SubmitReportCommand
/// Validates entity ID, report metadata, and uploaded file (type, size, content)
/// </summary>
public class SubmitReportCommandValidator : AbstractValidator<SubmitReportCommand>
{
    public SubmitReportCommandValidator()
    {
        // Validate EntityId
        RuleFor(x => x.EntityId)
            .NotEmpty().WithMessage("Entity ID is required")
            .GreaterThan(0).WithMessage("Entity ID must be positive");

        // Validate ReportType
        RuleFor(x => x.ReportType)
            .NotEmpty().WithMessage("Report type is required")
            .MaximumLength(250).WithMessage("Report type cannot exceed 250 characters");

        // Validate ReportingPeriod
        RuleFor(x => x.ReportingPeriod)
            .NotEmpty().WithMessage("Reporting period is required")
            .MaximumLength(100).WithMessage("Reporting period cannot exceed 100 characters")
            .Matches(@"^(Q[1-4]_\d{4}|Annual_\d{4}|Monthly_\d{2}_\d{4})$")
            .WithMessage("Reporting period must match format: Q1_2025, Annual_2025, or Monthly_01_2025");

        // Validate File - not null
        RuleFor(x => x.File)
            .NotNull().WithMessage("Report file is required");

        // Validate File - must be XLSX
        RuleFor(x => x.File)
            .Must(file => file != null && FileValidator.IsValidXlsxFile(file))
            .WithMessage("File must be a valid XLSX file. Ensure the file is not renamed from another format.")
            .When(x => x.File != null);

        // Validate File - size limit
        RuleFor(x => x.File)
            .Must(file => file != null && FileValidator.IsFileSizeValid(file))
            .WithMessage($"File size must be between 1 byte and {FileValidator.MaxFileSize / 1024 / 1024} MB")
            .When(x => x.File != null);

        // Validate File - content type
        RuleFor(x => x.File)
            .Must(file => file != null && 
                  string.Equals(file.ContentType, 
                               "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                               StringComparison.OrdinalIgnoreCase))
            .WithMessage("File content type must be 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'")
            .When(x => x.File != null);

        // Validate File - extension
        RuleFor(x => x.File)
            .Must(file => file != null && 
                  Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            .WithMessage("File extension must be .xlsx")
            .When(x => x.File != null);
    }
}

