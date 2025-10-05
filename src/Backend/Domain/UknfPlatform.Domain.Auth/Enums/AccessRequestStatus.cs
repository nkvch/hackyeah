namespace UknfPlatform.Domain.Auth.Enums;

/// <summary>
/// Status of an access request in the authorization workflow.
/// Story 2.1: Automatic Access Request Creation
/// </summary>
public enum AccessRequestStatus
{
    /// <summary>
    /// Initial status when automatically created after account activation.
    /// User can still edit the request. Not visible to UKNF reviewers.
    /// </summary>
    Working,
    
    /// <summary>
    /// User has submitted the request. Visible to UKNF reviewers.
    /// Waiting for review.
    /// </summary>
    New,
    
    /// <summary>
    /// UKNF has approved the request and granted permissions.
    /// </summary>
    Accepted,
    
    /// <summary>
    /// UKNF has rejected or blocked the request.
    /// </summary>
    Blocked,
    
    /// <summary>
    /// User has made changes after initial submission.
    /// Requires re-review by UKNF.
    /// </summary>
    Updated
}

