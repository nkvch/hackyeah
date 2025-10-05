using Microsoft.AspNetCore.Http;

namespace UknfPlatform.Application.Communication.Common;

/// <summary>
/// Utility class for validating file uploads
/// </summary>
public static class FileValidator
{
    private const long MAX_FILE_SIZE = 104_857_600; // 100 MB in bytes
    private const string XLSX_CONTENT_TYPE = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    /// <summary>
    /// Validates if a file is a valid XLSX file
    /// Checks content type, extension, and magic bytes (file signature)
    /// </summary>
    /// <param name="file">The uploaded file</param>
    /// <returns>True if valid XLSX file, false otherwise</returns>
    public static bool IsValidXlsxFile(IFormFile file)
    {
        if (file == null)
            return false;

        // Check content type
        if (!string.Equals(file.ContentType, XLSX_CONTENT_TYPE, StringComparison.OrdinalIgnoreCase))
            return false;

        // Check file extension (case-insensitive)
        var extension = Path.GetExtension(file.FileName);
        if (!string.Equals(extension, ".xlsx", StringComparison.OrdinalIgnoreCase))
            return false;

        // Check file signature (magic bytes)
        // XLSX files are ZIP archives, so they start with PK header (50 4B 03 04)
        try
        {
            using var stream = file.OpenReadStream();
            var header = new byte[4];
            var bytesRead = stream.Read(header, 0, 4);

            if (bytesRead < 4)
                return false;

            // Check for ZIP file signature (PK\x03\x04)
            if (header[0] != 0x50 || header[1] != 0x4B || header[2] != 0x03 || header[3] != 0x04)
                return false;

            // Reset stream position for subsequent reads
            stream.Position = 0;

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates if file size is within the acceptable limit
    /// </summary>
    /// <param name="file">The uploaded file</param>
    /// <returns>True if file size is acceptable, false otherwise</returns>
    public static bool IsFileSizeValid(IFormFile file)
    {
        if (file == null)
            return false;

        return file.Length > 0 && file.Length <= MAX_FILE_SIZE;
    }

    /// <summary>
    /// Gets the maximum allowed file size in bytes
    /// </summary>
    public static long MaxFileSize => MAX_FILE_SIZE;
}

