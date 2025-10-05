namespace UknfPlatform.Application.Shared.Interfaces;

/// <summary>
/// Service for storing and retrieving files from object storage (Azure Blob Storage, AWS S3, or local storage)
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Uploads a file to storage
    /// </summary>
    /// <param name="stream">File content stream</param>
    /// <param name="fileName">Original file name</param>
    /// <param name="contentType">MIME content type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Unique storage key/path for retrieving the file</returns>
    Task<string> UploadFileAsync(
        Stream stream, 
        string fileName, 
        string contentType, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads a file from storage
    /// </summary>
    /// <param name="storageKey">Unique storage key/path</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>File content stream</returns>
    Task<Stream> DownloadFileAsync(
        string storageKey, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a file from storage
    /// </summary>
    /// <param name="storageKey">Unique storage key/path</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteFileAsync(
        string storageKey, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a temporary signed URL for direct file access (for downloads)
    /// </summary>
    /// <param name="storageKey">Unique storage key/path</param>
    /// <param name="expiryMinutes">URL expiry time in minutes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Temporary signed URL</returns>
    Task<string> GetFileUrlAsync(
        string storageKey, 
        int expiryMinutes, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a file exists in storage
    /// </summary>
    /// <param name="storageKey">Unique storage key/path</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if file exists, false otherwise</returns>
    Task<bool> FileExistsAsync(
        string storageKey, 
        CancellationToken cancellationToken = default);
}

