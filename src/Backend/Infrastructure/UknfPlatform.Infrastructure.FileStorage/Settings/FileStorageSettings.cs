namespace UknfPlatform.Infrastructure.FileStorage.Settings;

/// <summary>
/// Configuration settings for file storage
/// </summary>
public class FileStorageSettings
{
    /// <summary>
    /// Storage type: Local, AzureBlob, or S3
    /// </summary>
    public string StorageType { get; set; } = "Local";

    /// <summary>
    /// Base path for local file storage
    /// </summary>
    public string LocalStoragePath { get; set; } = "./storage/files";

    /// <summary>
    /// Azure Blob Storage connection string (if using Azure)
    /// </summary>
    public string? AzureBlobConnectionString { get; set; }

    /// <summary>
    /// Azure Blob Storage container name (if using Azure)
    /// </summary>
    public string? AzureBlobContainerName { get; set; }

    /// <summary>
    /// AWS S3 bucket name (if using S3)
    /// </summary>
    public string? S3BucketName { get; set; }

    /// <summary>
    /// AWS S3 region (if using S3)
    /// </summary>
    public string? S3Region { get; set; }
}

