using Microsoft.Extensions.Configuration;
using UIPS.API.Services;
using Xunit;
using FluentAssertions;
using System.Text;

namespace UIPS.API.Tests.Services;

public class FileServiceTests : IDisposable
{
    private readonly LocalFileService _service;
    private readonly string _testUploadPath;

    public FileServiceTests()
    {
        _testUploadPath = Path.Combine(Path.GetTempPath(), "UIPS_Tests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testUploadPath);

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string> {
                {"Storage:UploadPath", _testUploadPath}
            }!)
            .Build();

        _service = new LocalFileService(configuration);
    }

    [Fact]
    public async Task SaveFileAsync_CreatesFileAndReturnsPath()
    {
        // Arrange
        var content = "Test content";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var fileName = "test.txt";

        // Act
        var result = await _service.SaveFileAsync(stream, fileName);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().EndWith(".txt");
    }

    [Fact]
    public async Task GetFileStreamAsync_NonExistentFile_ReturnsNull()
    {
        // Act
        var result = await _service.GetFileStreamAsync("nonexistent/path.txt");

        // Assert
        result.Should().BeNull();
    }

    public void Dispose()
    {
        if (Directory.Exists(_testUploadPath))
        {
            Directory.Delete(_testUploadPath, true);
        }
    }
}