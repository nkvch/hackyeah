namespace UknfPlatform.Domain.Communication.Enums;

/// <summary>
/// Report validation status lifecycle
/// </summary>
public enum ValidationStatus
{
    /// <summary>
    /// Initial status after upload, before transmission to validation service
    /// </summary>
    Working,
    
    /// <summary>
    /// Report sent to external validation service
    /// </summary>
    Transmitted,
    
    /// <summary>
    /// Validation in progress by external service
    /// </summary>
    Ongoing,
    
    /// <summary>
    /// Validation completed successfully
    /// </summary>
    Successful,
    
    /// <summary>
    /// Validation failed with errors
    /// </summary>
    ValidationErrors,
    
    /// <summary>
    /// Technical error during validation process
    /// </summary>
    TechnicalError,
    
    /// <summary>
    /// Validation exceeded 24-hour timeout
    /// </summary>
    TimeoutError,
    
    /// <summary>
    /// Manually challenged by UKNF employee
    /// </summary>
    ContestedByUKNF
}

