namespace UknfPlatform.Domain.Shared.Exceptions;

/// <summary>
/// Exception thrown when a resource conflict occurs (e.g., duplicate email)
/// </summary>
public class ConflictException : Exception
{
    public ConflictException(string message)
        : base(message)
    {
    }

    public ConflictException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

