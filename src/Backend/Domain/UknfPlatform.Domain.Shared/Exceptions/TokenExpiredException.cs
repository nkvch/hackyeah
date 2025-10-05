namespace UknfPlatform.Domain.Shared.Exceptions;

/// <summary>
/// Exception thrown when an activation or reset token has expired
/// </summary>
public class TokenExpiredException : Exception
{
    public TokenExpiredException(string message) : base(message)
    {
    }

    public TokenExpiredException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}

