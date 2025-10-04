namespace UknfPlatform.Application.Shared.Interfaces;

/// <summary>
/// Service for encrypting and decrypting sensitive data
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// Encrypts a plain text string
    /// </summary>
    string Encrypt(string plainText);

    /// <summary>
    /// Decrypts an encrypted string
    /// </summary>
    string Decrypt(string encryptedText);
}

