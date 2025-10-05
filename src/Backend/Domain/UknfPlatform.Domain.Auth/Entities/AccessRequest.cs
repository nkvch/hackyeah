using UknfPlatform.Domain.Auth.Enums;

namespace UknfPlatform.Domain.Auth.Entities;

/// <summary>
/// Represents a user's request for access and permissions to the platform.
/// Story 2.1: Automatic Access Request Creation
/// Epic 2: Authorization & Access Requests
/// </summary>
public class AccessRequest
{
    private AccessRequest() { } // EF Core constructor

    private AccessRequest(Guid userId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Status = AccessRequestStatus.Working;
        CreatedDate = DateTime.UtcNow;
        UpdatedDate = DateTime.UtcNow;
    }

    // Properties
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public AccessRequestStatus Status { get; private set; }
    public DateTime? SubmittedDate { get; private set; }
    public Guid? ReviewedByUserId { get; private set; }
    public DateTime? ReviewedDate { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public DateTime UpdatedDate { get; private set; }

    // Navigation properties
    public User? User { get; private set; }
    public User? ReviewedByUser { get; private set; }

    /// <summary>
    /// Factory method: Creates a new access request for a user after account activation.
    /// Initial status is "Working" - user can edit before submitting.
    /// </summary>
    public static AccessRequest CreateForUser(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        return new AccessRequest(userId);
    }

    /// <summary>
    /// Submits the access request for UKNF review.
    /// Changes status from Working to New.
    /// </summary>
    public void Submit()
    {
        if (Status != AccessRequestStatus.Working && Status != AccessRequestStatus.Updated)
            throw new InvalidOperationException($"Cannot submit request in status: {Status}");

        Status = Status == AccessRequestStatus.Working 
            ? AccessRequestStatus.New 
            : AccessRequestStatus.Updated;
        SubmittedDate = DateTime.UtcNow;
        UpdatedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// UKNF reviewer accepts the access request and grants permissions.
    /// </summary>
    public void Accept(Guid reviewerUserId)
    {
        if (Status != AccessRequestStatus.New && Status != AccessRequestStatus.Updated)
            throw new InvalidOperationException($"Cannot accept request in status: {Status}");

        if (reviewerUserId == Guid.Empty)
            throw new ArgumentException("Reviewer user ID cannot be empty", nameof(reviewerUserId));

        Status = AccessRequestStatus.Accepted;
        ReviewedByUserId = reviewerUserId;
        ReviewedDate = DateTime.UtcNow;
        UpdatedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// UKNF reviewer blocks/rejects the access request.
    /// </summary>
    public void Block(Guid reviewerUserId)
    {
        if (reviewerUserId == Guid.Empty)
            throw new ArgumentException("Reviewer user ID cannot be empty", nameof(reviewerUserId));

        Status = AccessRequestStatus.Blocked;
        ReviewedByUserId = reviewerUserId;
        ReviewedDate = DateTime.UtcNow;
        UpdatedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the request as updated after user makes changes post-submission.
    /// Changes status to Updated, requiring re-review.
    /// </summary>
    public void MarkAsUpdated()
    {
        if (Status != AccessRequestStatus.New && Status != AccessRequestStatus.Accepted)
            throw new InvalidOperationException($"Cannot update request in status: {Status}");

        Status = AccessRequestStatus.Updated;
        UpdatedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if the request can be edited by the user.
    /// Only editable in Working status.
    /// </summary>
    public bool IsEditable() => Status == AccessRequestStatus.Working;

    /// <summary>
    /// Checks if the request is visible to UKNF reviewers.
    /// Only visible after submission (not in Working status).
    /// </summary>
    public bool IsVisibleToReviewers() => Status != AccessRequestStatus.Working;

    /// <summary>
    /// Checks if the request is pending review.
    /// </summary>
    public bool IsPendingReview() => Status == AccessRequestStatus.New || Status == AccessRequestStatus.Updated;
}

