namespace UknfPlatform.Domain.Shared.Exceptions;

/// <summary>
/// Exception thrown when a requested resource is not found
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string resourceType, object resourceId)
        : base($"{resourceType} with ID '{resourceId}' was not found")
    {
    }

    public NotFoundException(string message)
        : base(message)
    {
    }

    public NotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

