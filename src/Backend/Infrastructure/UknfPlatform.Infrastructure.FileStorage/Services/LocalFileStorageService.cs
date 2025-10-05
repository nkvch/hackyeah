using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UknfPlatform.Application.Shared.Interfaces;
using UknfPlatform.Infrastructure.FileStorage.Settings;

namespace UknfPlatform.Infrastructure.FileStorage.Services;

/// <summary>
/// Local file system storage implementation (for development and testing)
/// In production, should be replaced with Azure Blob Storage or AWS S3
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly FileStorageSettings _settings;
    private readonly ILogger<LocalFileStorageService> _logger;
    private readonly string _storagePath;

    public LocalFileStorageService(
        IOptions<FileStorageSettings> settings,
        ILogger<LocalFileStorageService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        _storagePath = Path.GetFullPath(_settings.LocalStoragePath);

        // Ensure storage directory exists
        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
            _logger.LogInformation("Created local storage directory: {StoragePath}", _storagePath);
        }
    }

    public async Task<string> UploadFileAsync(
        Stream stream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Generate unique storage key: reports/YYYY/MM/guid_originalfilename
            var year = DateTime.UtcNow.Year;
            var month = DateTime.UtcNow.Month.ToString("D2");
            var uniqueId = Guid.NewGuid().ToString("N");
            var safeFileName = Path.GetFileName(fileName); // Sanitize filename
            
            var storageKey = $"reports/{year}/{month}/{uniqueId}_{safeFileName}";
            var fullPath = Path.Combine(_storagePath, storageKey);

            // Ensure directory exists
            var directory = Path.GetDirectoryName(fullPath);
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Write file to disk
            using (var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await stream.CopyToAsync(fileStream, cancellationToken);
            }

            _logger.LogInformation("File uploaded successfully. Storage key: {StorageKey}, Size: {Size} bytes",
                storageKey, stream.Length);

            return storageKey;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload file: {FileName}", fileName);
            throw;
        }
    }

    public async Task<Stream> DownloadFileAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var fullPath = Path.Combine(_storagePath, storageKey);

            if (!File.Exists(fullPath))
            {
                _logger.LogWarning("File not found: {StorageKey}", storageKey);
                throw new FileNotFoundException($"File not found: {storageKey}");
            }

            // Return file stream (caller is responsible for disposing)
            var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            
            _logger.LogInformation("File downloaded successfully. Storage key: {StorageKey}", storageKey);
            
            return await Task.FromResult(stream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download file: {StorageKey}", storageKey);
            throw;
        }
    }

    public Task DeleteFileAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var fullPath = Path.Combine(_storagePath, storageKey);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("File deleted successfully. Storage key: {StorageKey}", storageKey);
            }
            else
            {
                _logger.LogWarning("File not found for deletion: {StorageKey}", storageKey);
            }

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file: {StorageKey}", storageKey);
            throw;
        }
    }

    public Task<string> GetFileUrlAsync(
        string storageKey,
        int expiryMinutes,
        CancellationToken cancellationToken = default)
    {
        // For local storage, return file path
        // In production (Azure/S3), this would return a signed URL
        var fullPath = Path.Combine(_storagePath, storageKey);
        
        _logger.LogInformation("Generated file URL for: {StorageKey}", storageKey);
        
        return Task.FromResult($"file://{fullPath}");
    }

    public Task<bool> FileExistsAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_storagePath, storageKey);
        var exists = File.Exists(fullPath);
        
        return Task.FromResult(exists);
    }
}

