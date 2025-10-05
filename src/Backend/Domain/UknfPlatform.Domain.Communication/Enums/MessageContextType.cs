namespace UknfPlatform.Domain.Communication.Enums;

/// <summary>
/// Context in which a message is sent
/// Story 5.1: Messages can be standalone or linked to access requests, reports, or cases
/// </summary>
public enum MessageContextType
{
    /// <summary>
    /// Standalone message not linked to any specific context
    /// </summary>
    Standalone,
    
    /// <summary>
    /// Message related to an access request
    /// </summary>
    AccessRequest,
    
    /// <summary>
    /// Message related to a report
    /// </summary>
    Report,
    
    /// <summary>
    /// Message related to an administrative case
    /// </summary>
    Case
}

