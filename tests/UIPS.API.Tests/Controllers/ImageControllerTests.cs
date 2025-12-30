using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;
using UIPS.API.Controllers;
using UIPS.API.Models;
using UIPS.API.Services;
using Xunit;
using FluentAssertions;

namespace UIPS.API.Tests.Controllers;

public class ImageControllerTests : IDisposable
{
    private readonly UipsDbContext _context;
    private readonly Mock<IFileService> _fileServiceMock;
    private readonly ImageController _controller;
    private readonly int _testUserId = 1;

    public ImageControllerTests()
    {
        // 使用内存数据库
        var options = new DbContextOptionsBuilder<UipsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new UipsDbContext(options);

        // 创建测试用户
        _context.Users.Add(new User
        {
            Id = _testUserId,
            UserName = "testuser",
            PasswordHash = "hash",
            Role = "User"
        });
        _context.SaveChanges();

        // Mock FileService
        _fileServiceMock = new Mock<IFileService>();

        // 创建 Controller
        _controller = new ImageController(_context, _fileServiceMock.Object);

        // 模拟用户身份
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString()),
            new Claim(ClaimTypes.Name, "testuser")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task GetUniqueFileNames_ReturnsDistinctFileNames()
    {
        // Arrange
        _context.Images.AddRange(
            new Image { OriginalFileName = "test1.jpg", StoredPath = "path1", OwnerId = _testUserId, FileSize = 100 },
            new Image { OriginalFileName = "test1.jpg", StoredPath = "path2", OwnerId = _testUserId, FileSize = 100 },
            new Image { OriginalFileName = "test2.jpg", StoredPath = "path3", OwnerId = _testUserId, FileSize = 100 }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetUniqueFileNames();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var fileNames = okResult!.Value as List<string>;
        fileNames.Should().HaveCount(2);
        fileNames.Should().Contain("test1.jpg");
        fileNames.Should().Contain("test2.jpg");
    }

    [Fact]
    public async Task GetImagesByFileName_ReturnsImagesWithSameFileName()
    {
        // Arrange
        var fileName = "test.jpg";
        _context.Images.AddRange(
            new Image { OriginalFileName = fileName, StoredPath = "path1", OwnerId = _testUserId, FileSize = 100 },
            new Image { OriginalFileName = fileName, StoredPath = "path2", OwnerId = _testUserId, FileSize = 200 },
            new Image { OriginalFileName = "other.jpg", StoredPath = "path3", OwnerId = _testUserId, FileSize = 300 }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetImagesByFileName(fileName);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var images = okResult!.Value as List<UIPS.API.DTOs.ImageDto>;
        images.Should().HaveCount(2);
        images.Should().AllSatisfy(img => img.OriginalFileName.Should().Be(fileName));
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}