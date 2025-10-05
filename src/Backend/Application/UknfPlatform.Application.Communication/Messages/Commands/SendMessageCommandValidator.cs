using FluentValidation;

namespace UknfPlatform.Application.Communication.Messages.Commands;

/// <summary>
/// Validator for SendMessageCommand
/// Story 5.1: Validates message content and attachments
/// </summary>
public class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    private const long MAX_TOTAL_FILE_SIZE = 104_857_600; // 100 MB
    private static readonly string[] AllowedExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".csv", ".txt", ".mp3", ".zip" };
    
    public SendMessageCommandValidator()
    {
        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Subject is required")
            .MaximumLength(500).WithMessage("Subject cannot exceed 500 characters");
        
        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("Message body is required")
            .MaximumLength(10000).WithMessage("Message body cannot exceed 10,000 characters");
        
        RuleFor(x => x.RecipientUserIds)
            .NotEmpty().WithMessage("At least one recipient is required")
            .Must(recipients => recipients.All(id => id != Guid.Empty))
            .WithMessage("All recipient IDs must be valid");
        
        RuleFor(x => x.ContextType)
            .NotEmpty().WithMessage("Context type is required")
            .Must(BeValidContextType).WithMessage("Invalid context type. Must be: Standalone, AccessRequest, Report, or Case");
        
        RuleFor(x => x.ContextId)
            .NotEmpty().When(x => x.ContextType != "Standalone")
            .WithMessage("Context ID is required when context type is not Standalone");
        
        // File attachments validation
        RuleFor(x => x.Attachments)
            .Must(HaveValidFileExtensions).WithMessage($"Only these file formats are allowed: {string.Join(", ", AllowedExtensions)}")
            .Must(HaveValidTotalSize).WithMessage($"Total file size cannot exceed 100 MB");
    }
    
    private bool BeValidContextType(string contextType)
    {
        return contextType is "Standalone" or "AccessRequest" or "Report" or "Case";
    }
    
    private bool HaveValidFileExtensions(List<Microsoft.AspNetCore.Http.IFormFile> files)
    {
        if (files == null || !files.Any())
            return true; // No files is valid
        
        foreach (var file in files)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
                return false;
        }
        
        return true;
    }
    
    private bool HaveValidTotalSize(List<Microsoft.AspNetCore.Http.IFormFile> files)
    {
        if (files == null || !files.Any())
            return true; // No files is valid
        
        var totalSize = files.Sum(f => f.Length);
        return totalSize <= MAX_TOTAL_FILE_SIZE;
    }
}

