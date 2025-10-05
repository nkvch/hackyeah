using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using UknfPlatform.Application.Communication.Common;
using Xunit;

namespace UknfPlatform.UnitTests.Application.Communication;

public class FileValidatorTests
{
    [Fact]
    public void IsValidXlsxFile_ValidXlsxFile_ReturnsTrue()
    {
        // Arrange
        var mockFile = CreateMockXlsxFile("report.xlsx", 1024 * 1024, validMagicBytes: true);

        // Act
        var result = FileValidator.IsValidXlsxFile(mockFile);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidXlsxFile_NullFile_ReturnsFalse()
    {
        // Act
        var result = FileValidator.IsValidXlsxFile(null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidXlsxFile_EmptyFile_ReturnsFalse()
    {
        // Arrange
        var mockFile = CreateMockXlsxFile("empty.xlsx", 0, validMagicBytes: true);

        // Act
        var result = FileValidator.IsValidXlsxFile(mockFile);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidXlsxFile_FileExceedsMaxSize_ReturnsFalse()
    {
        // Arrange
        var mockFile = CreateMockXlsxFile("huge.xlsx", 101 * 1024 * 1024, validMagicBytes: true); // 101 MB

        // Act
        var result = FileValidator.IsValidXlsxFile(mockFile);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidXlsxFile_InvalidContentType_ReturnsFalse()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("report.pdf");
        mockFile.Setup(f => f.Length).Returns(1024);
        mockFile.Setup(f => f.ContentType).Returns("application/pdf");
        var stream = new MemoryStream(new byte[] { 0x50, 0x4B, 0x03, 0x04 });
        mockFile.Setup(f => f.OpenReadStream()).Returns(stream);

        // Act
        var result = FileValidator.IsValidXlsxFile(mockFile.Object);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidXlsxFile_InvalidMagicBytes_ReturnsFalse()
    {
        // Arrange
        var mockFile = CreateMockXlsxFile("report.xlsx", 1024, validMagicBytes: false);

        // Act
        var result = FileValidator.IsValidXlsxFile(mockFile);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(0x00, 0x00, 0x00, 0x00)] // Invalid
    [InlineData(0x25, 0x50, 0x44, 0x46)] // PDF signature
    [InlineData(0x89, 0x50, 0x4E, 0x47)] // PNG signature
    [InlineData(0xFF, 0xD8, 0xFF, 0xE0)] // JPEG signature
    public void IsValidXlsxFile_VariousInvalidMagicBytes_ReturnsFalse(byte b1, byte b2, byte b3, byte b4)
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("report.xlsx");
        mockFile.Setup(f => f.Length).Returns(1024);
        mockFile.Setup(f => f.ContentType).Returns("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        var stream = new MemoryStream(new byte[] { b1, b2, b3, b4 });
        mockFile.Setup(f => f.OpenReadStream()).Returns(stream);

        // Act
        var result = FileValidator.IsValidXlsxFile(mockFile.Object);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidXlsxFile_ValidMagicBytes_PassesValidation()
    {
        // Arrange - XLSX files are ZIP archives with PK header
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("report.xlsx");
        mockFile.Setup(f => f.Length).Returns(1024);
        mockFile.Setup(f => f.ContentType).Returns("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        var stream = new MemoryStream(new byte[] { 0x50, 0x4B, 0x03, 0x04 }); // PK\x03\x04
        mockFile.Setup(f => f.OpenReadStream()).Returns(stream);

        // Act
        var result = FileValidator.IsValidXlsxFile(mockFile.Object);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(100)]
    [InlineData(1024)]
    [InlineData(1024 * 1024)] // 1 MB
    [InlineData(50 * 1024 * 1024)] // 50 MB
    [InlineData(100 * 1024 * 1024)] // 100 MB (max allowed)
    public void IsValidXlsxFile_ValidFileSizes_PassesValidation(long fileSize)
    {
        // Arrange
        var mockFile = CreateMockXlsxFile("report.xlsx", fileSize, validMagicBytes: true);

        // Act
        var result = FileValidator.IsValidXlsxFile(mockFile);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData((100 * 1024 * 1024) + 1)] // Just over 100 MB
    [InlineData(200 * 1024 * 1024)] // 200 MB
    [InlineData(1024 * 1024 * 1024)] // 1 GB
    public void IsValidXlsxFile_ExcessiveFileSizes_FailsValidation(long fileSize)
    {
        // Arrange
        var mockFile = CreateMockXlsxFile("huge_report.xlsx", fileSize, validMagicBytes: true);

        // Act
        var result = FileValidator.IsValidXlsxFile(mockFile);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidXlsxFile_StreamReadThrowsException_ReturnsFalse()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("report.xlsx");
        mockFile.Setup(f => f.Length).Returns(1024);
        mockFile.Setup(f => f.ContentType).Returns("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        mockFile.Setup(f => f.OpenReadStream()).Throws(new IOException("Stream read error"));

        // Act
        var result = FileValidator.IsValidXlsxFile(mockFile.Object);

        // Assert
        result.Should().BeFalse();
    }

    // Helper methods
    private IFormFile CreateMockXlsxFile(string fileName, long fileSize, bool validMagicBytes)
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.Length).Returns(fileSize);
        mockFile.Setup(f => f.ContentType).Returns("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

        if (fileSize > 0)
        {
            var data = new byte[Math.Min(fileSize, 1024)]; // Create at least 4 bytes for magic bytes
            if (validMagicBytes)
            {
                data[0] = 0x50; // 'P'
                data[1] = 0x4B; // 'K'
                data[2] = 0x03;
                data[3] = 0x04;
            }
            else
            {
                data[0] = 0x00;
                data[1] = 0x00;
                data[2] = 0x00;
                data[3] = 0x00;
            }
            var stream = new MemoryStream(data);
            mockFile.Setup(f => f.OpenReadStream()).Returns(stream);
        }
        else
        {
            mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());
        }

        return mockFile.Object;
    }
}

