namespace UknfPlatform.Domain.Shared.Exceptions;

/// <summary>
/// Exception thrown when an activation or reset token is invalid
/// </summary>
public class InvalidTokenException : Exception
{
    public InvalidTokenException(string message) : base(message)
    {
    }

    public InvalidTokenException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}

