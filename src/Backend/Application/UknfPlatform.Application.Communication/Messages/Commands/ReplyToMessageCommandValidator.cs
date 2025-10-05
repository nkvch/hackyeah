using FluentValidation;

namespace UknfPlatform.Application.Communication.Messages.Commands;

/// <summary>
/// Validator for ReplyToMessageCommand
/// Story 5.3: Reply to Message
/// </summary>
public class ReplyToMessageCommandValidator : AbstractValidator<ReplyToMessageCommand>
{
    private const int MaxBodyLength = 10000;
    private const long MaxTotalAttachmentSize = 104_857_600; // 100 MB
    
    private static readonly string[] AllowedExtensions = new[]
    {
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".csv", ".txt", ".mp3", ".zip"
    };

    public ReplyToMessageCommandValidator()
    {
        RuleFor(x => x.ParentMessageId)
            .NotEmpty()
            .WithMessage("Parent message ID is required");

        RuleFor(x => x.Body)
            .NotEmpty()
            .WithMessage("Reply body is required")
            .MaximumLength(MaxBodyLength)
            .WithMessage($"Reply body cannot exceed {MaxBodyLength} characters");

        // Validate attachments if provided
        When(x => x.Attachments != null && x.Attachments.Any(), () =>
        {
            RuleFor(x => x.Attachments)
                .Must(files => files!.All(f => IsAllowedExtension(f.FileName)))
                .WithMessage($"Only the following file types are allowed: {string.Join(", ", AllowedExtensions)}")
                .Must(files => files!.Sum(f => f.Length) <= MaxTotalAttachmentSize)
                .WithMessage($"Total attachment size cannot exceed {MaxTotalAttachmentSize / 1024 / 1024} MB");
        });
    }

    private static bool IsAllowedExtension(string fileName)
    {
        var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
        return extension != null && AllowedExtensions.Contains(extension);
    }
}

