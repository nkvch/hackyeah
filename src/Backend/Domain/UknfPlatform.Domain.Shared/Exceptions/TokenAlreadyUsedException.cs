namespace UknfPlatform.Domain.Shared.Exceptions;

/// <summary>
/// Exception thrown when attempting to use an already-used activation or reset token
/// </summary>
public class TokenAlreadyUsedException : Exception
{
    public TokenAlreadyUsedException(string message) : base(message)
    {
    }

    public TokenAlreadyUsedException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}

