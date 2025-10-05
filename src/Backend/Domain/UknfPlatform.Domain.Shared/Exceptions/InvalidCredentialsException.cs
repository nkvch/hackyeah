namespace UknfPlatform.Domain.Shared.Exceptions;

/// <summary>
/// Exception thrown when login credentials are invalid
/// Generic message used for security (don't reveal if email exists)
/// </summary>
public class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException()
        : base("Invalid email or password")
    {
    }

    public InvalidCredentialsException(string message)
        : base(message)
    {
    }

    public InvalidCredentialsException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

