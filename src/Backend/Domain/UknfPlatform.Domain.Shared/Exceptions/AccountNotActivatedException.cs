namespace UknfPlatform.Domain.Shared.Exceptions;

/// <summary>
/// Exception thrown when attempting to login with an inactive account
/// User must activate their account via email before logging in
/// </summary>
public class AccountNotActivatedException : Exception
{
    public AccountNotActivatedException()
        : base("Please activate your account. Check your email for the activation link.")
    {
    }

    public AccountNotActivatedException(string message)
        : base(message)
    {
    }

    public AccountNotActivatedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

