using FlowApplicationApp.Data;
using FlowApplicationApp.Data.DomainModels;
using FlowApplicationApp.Infrastructure.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace FlowApplicationApp.Tests.Infrastructure.Services;

public class MemberCodeOfConductServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<MemberCodeOfConductService> _logger;
    private readonly MemberCodeOfConductService _service;
    private readonly string _testUploadPath;

    public MemberCodeOfConductServiceTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);

        // Setup mocked environment
        _environment = Substitute.For<IWebHostEnvironment>();
        _testUploadPath = Path.Combine(Path.GetTempPath(), "flow-test-uploads", Guid.NewGuid().ToString());
        _environment.ContentRootPath.Returns(_testUploadPath);

        // Setup logger
        _logger = Substitute.For<ILogger<MemberCodeOfConductService>>();

        _service = new MemberCodeOfConductService(_context, _environment, _logger);

        // Create test upload directory
        if (!Directory.Exists(_testUploadPath))
        {
            Directory.CreateDirectory(_testUploadPath);
        }
    }

    [Fact]
    public async Task UploadMemberCodeOfConductAsync_WithValidPdf_ShouldSaveFileAndUpdateDatabase()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var member = new FlowMember
        {
            Id = memberId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            UserName = "johndoe"
        };
        _context.FlowMembers.Add(member);
        await _context.SaveChangesAsync();

        var file = CreateMockPdfFile("test.pdf", 1024);

        // Act
        var result = await _service.UploadMemberCodeOfConductAsync(file, memberId);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("code-of-conduct/members", result);
        
        var updatedMember = await _context.FlowMembers.FindAsync(memberId);
        Assert.NotNull(updatedMember);
        Assert.True(updatedMember.HasUploadedCodeOfConduct);
        Assert.NotNull(updatedMember.CodeOfConductUploadedAt);
        Assert.NotNull(updatedMember.CodeOfConductPdfPath);
    }

    [Fact]
    public async Task UploadMemberCodeOfConductAsync_WithNullFile_ShouldThrowArgumentException()
    {
        // Arrange
        var memberId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.UploadMemberCodeOfConductAsync(null!, memberId));
    }

    [Fact]
    public async Task UploadMemberCodeOfConductAsync_WithOversizedFile_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var member = new FlowMember
        {
            Id = memberId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            UserName = "johndoe"
        };
        _context.FlowMembers.Add(member);
        await _context.SaveChangesAsync();

        var file = CreateMockPdfFile("large.pdf", 11 * 1024 * 1024); // 11MB

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _service.UploadMemberCodeOfConductAsync(file, memberId));
        Assert.Contains("exceeds maximum", exception.Message);
    }

    [Fact]
    public async Task UploadMemberCodeOfConductAsync_WithNonPdfFile_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var member = new FlowMember
        {
            Id = memberId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            UserName = "johndoe"
        };
        _context.FlowMembers.Add(member);
        await _context.SaveChangesAsync();

        var file = CreateMockFile("test.txt", 1024, ".txt");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _service.UploadMemberCodeOfConductAsync(file, memberId));
        Assert.Contains("PDF files are allowed", exception.Message);
    }

    [Fact]
    public async Task UploadMemberCodeOfConductAsync_WithNonExistentMember_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var file = CreateMockPdfFile("test.pdf", 1024);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _service.UploadMemberCodeOfConductAsync(file, memberId));
        Assert.Contains("Member not found", exception.Message);
    }

    [Fact]
    public async Task HasUploadedCodeOfConductAsync_WhenMemberHasUploaded_ShouldReturnTrue()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var member = new FlowMember
        {
            Id = memberId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            UserName = "johndoe",
            HasUploadedCodeOfConduct = true
        };
        _context.FlowMembers.Add(member);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.HasUploadedCodeOfConductAsync(memberId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task HasUploadedCodeOfConductAsync_WhenMemberHasNotUploaded_ShouldReturnFalse()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var member = new FlowMember
        {
            Id = memberId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            UserName = "johndoe",
            HasUploadedCodeOfConduct = false
        };
        _context.FlowMembers.Add(member);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.HasUploadedCodeOfConductAsync(memberId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteMemberCodeOfConductAsync_WithExistingUpload_ShouldDeleteFileAndUpdateDatabase()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var member = new FlowMember
        {
            Id = memberId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            UserName = "johndoe",
            HasUploadedCodeOfConduct = true,
            CodeOfConductPdfPath = "test/path.pdf",
            CodeOfConductUploadedAt = DateTime.UtcNow
        };
        _context.FlowMembers.Add(member);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.DeleteMemberCodeOfConductAsync(memberId);

        // Assert
        Assert.True(result);
        
        var updatedMember = await _context.FlowMembers.FindAsync(memberId);
        Assert.NotNull(updatedMember);
        Assert.False(updatedMember.HasUploadedCodeOfConduct);
        Assert.Null(updatedMember.CodeOfConductUploadedAt);
        Assert.Null(updatedMember.CodeOfConductPdfPath);
    }

    [Fact]
    public async Task GetMemberCodeOfConductFileNameAsync_WithExistingUpload_ShouldReturnFileName()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var fileName = "CodeOfConduct_20251208.pdf";
        var member = new FlowMember
        {
            Id = memberId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            UserName = "johndoe",
            CodeOfConductPdfPath = $"code-of-conduct/members/{memberId}/{fileName}"
        };
        _context.FlowMembers.Add(member);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetMemberCodeOfConductFileNameAsync(memberId);

        // Assert
        Assert.Equal(fileName, result);
    }

    private IFormFile CreateMockPdfFile(string fileName, long size, string extension = ".pdf")
    {
        return CreateMockFile(fileName, size, extension);
    }

    private IFormFile CreateMockFile(string fileName, long size, string extension)
    {
        var file = Substitute.For<IFormFile>();
        file.FileName.Returns(fileName);
        file.Length.Returns(size);
        
        var stream = new MemoryStream(new byte[size]);
        file.OpenReadStream().Returns(stream);
        file.CopyToAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        return file;
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        
        // Clean up test files
        if (Directory.Exists(_testUploadPath))
        {
            try
            {
                Directory.Delete(_testUploadPath, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}
