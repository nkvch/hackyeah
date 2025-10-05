namespace UknfPlatform.Domain.Shared.Exceptions;

/// <summary>
/// Exception thrown when a user is not authenticated or authorized
/// </summary>
public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message)
        : base(message)
    {
    }

    public UnauthorizedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

