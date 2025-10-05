namespace UknfPlatform.Application.Auth.AccessRequests.DTOs;

/// <summary>
/// DTO for access request data.
/// Story 2.1: Automatic Access Request Creation
/// </summary>
public record AccessRequestDto
{
    public required Guid Id { get; init; }
    public required Guid UserId { get; init; }
    public required string Status { get; init; }
    public DateTime? SubmittedDate { get; init; }
    public DateTime CreatedDate { get; init; }
    
    // User data pre-populated from profile
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public required string PhoneNumber { get; init; }
    public required string PeselMasked { get; init; } // Shows last 4 digits only
    
    public bool IsEditable { get; init; }
    public bool IsVisibleToReviewers { get; init; }
}

