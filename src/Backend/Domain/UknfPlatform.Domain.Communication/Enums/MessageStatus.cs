namespace UknfPlatform.Domain.Communication.Enums;

/// <summary>
/// Status of a message indicating who needs to respond
/// Story 5.1, 5.4: Message Status Management
/// </summary>
public enum MessageStatus
{
    /// <summary>
    /// Message sent by External User, awaiting UKNF Employee response
    /// </summary>
    AwaitingUknfResponse,
    
    /// <summary>
    /// Message sent by UKNF Employee, awaiting External User response
    /// </summary>
    AwaitingUserResponse,
    
    /// <summary>
    /// Message has been replied to and is closed
    /// </summary>
    Closed
}

