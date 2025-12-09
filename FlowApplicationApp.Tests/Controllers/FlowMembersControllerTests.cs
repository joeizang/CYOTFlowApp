using System.Security.Claims;
using FlowApplicationApp.Controllers;
using FlowApplicationApp.Data;
using FlowApplicationApp.Data.DomainModels;
using FlowApplicationApp.Infrastructure.Services;
using FlowApplicationApp.ViewModels.FlowMembers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace FlowApplicationApp.Tests.Controllers;

public class FlowMembersControllerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IMemberCodeOfConductService _memberCocService;
    private readonly UserManager<FlowMember> _userManager;
    private readonly RoleManager<FlowRoles> _roleManager;
    private readonly ILogger<FlowMembersController> _logger;
    private readonly FlowMembersController _controller;

    public FlowMembersControllerTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);

        // Setup mocked services
        _memberCocService = Substitute.For<IMemberCodeOfConductService>();
        _logger = Substitute.For<ILogger<FlowMembersController>>();
        
        // Setup UserManager
        var userStore = Substitute.For<IUserStore<FlowMember>>();
        _userManager = Substitute.For<UserManager<FlowMember>>(
            userStore, null, null, null, null, null, null, null, null);

        // Setup RoleManager
        var roleStore = Substitute.For<IRoleStore<FlowRoles>>();
        _roleManager = Substitute.For<RoleManager<FlowRoles>>(
            roleStore, null, null, null, null);

        _controller = new FlowMembersController(
            _logger,
            _context,
            _memberCocService,
            _userManager,
            _roleManager
        );

        // Setup TempData
        _controller.TempData = Substitute.For<ITempDataDictionary>();
        
        // Setup URL helper - no argument matchers to avoid ambiguity
        var urlHelper = Substitute.For<IUrlHelper>();
        _controller.Url = urlHelper;
    }

    #region Code of Conduct Upload Tests

    [Fact]
    public async Task UploadCodeOfConduct_Get_WhenUserNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act
        var result = await _controller.UploadCodeOfConduct(CancellationToken.None);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task UploadCodeOfConduct_Get_WhenMemberHasNotUploaded_ShouldReturnViewWithModel()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        SetupAuthenticatedUser(memberId);
        _memberCocService.HasUploadedCodeOfConductAsync(memberId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _controller.UploadCodeOfConduct(CancellationToken.None);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<UploadMemberCodeOfConductViewModel>(viewResult.Model);
        Assert.False(model.IsSuccess);
    }

    [Fact]
    public async Task UploadCodeOfConduct_Get_WhenMemberHasUploaded_ShouldShowMessage()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        SetupAuthenticatedUser(memberId);
        _memberCocService.HasUploadedCodeOfConductAsync(memberId, Arg.Any<CancellationToken>())
            .Returns(true);
        _memberCocService.GetMemberCodeOfConductFileNameAsync(memberId, Arg.Any<CancellationToken>())
            .Returns("CodeOfConduct.pdf");

        // Act
        var result = await _controller.UploadCodeOfConduct(CancellationToken.None);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<UploadMemberCodeOfConductViewModel>(viewResult.Model);
        Assert.Contains("already uploaded", model.Message);
    }

    [Fact]
    public async Task UploadCodeOfConduct_Post_WithValidFile_ShouldUploadAndReturnSuccess()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        SetupAuthenticatedUser(memberId);
        
        var file = CreateMockPdfFile("test.pdf", 1024);
        var filePath = "code-of-conduct/members/test.pdf";
        
        _memberCocService.UploadMemberCodeOfConductAsync(file, memberId, Arg.Any<CancellationToken>())
            .Returns(filePath);

        // Act
        var result = await _controller.UploadCodeOfConduct(file, CancellationToken.None);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<UploadMemberCodeOfConductViewModel>(viewResult.Model);
        Assert.True(model.IsSuccess);
        Assert.Contains("uploaded successfully", model.Message);
        Assert.NotNull(model.DownloadUrl);
    }

    [Fact]
    public async Task UploadCodeOfConduct_Post_WithNullFile_ShouldReturnError()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        SetupAuthenticatedUser(memberId);

        // Act
        var result = await _controller.UploadCodeOfConduct(null, CancellationToken.None);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<UploadMemberCodeOfConductViewModel>(viewResult.Model);
        Assert.False(model.IsSuccess);
        Assert.Contains("select a PDF file", model.Message);
    }

    [Fact]
    public async Task UploadCodeOfConduct_Post_WhenServiceThrowsException_ShouldReturnError()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        SetupAuthenticatedUser(memberId);
        
        var file = CreateMockPdfFile("test.pdf", 1024);
        
        _memberCocService.UploadMemberCodeOfConductAsync(file, memberId, Arg.Any<CancellationToken>())
            .Returns<string>(x => throw new InvalidOperationException("File size exceeds limit"));

        // Act
        var result = await _controller.UploadCodeOfConduct(file, CancellationToken.None);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<UploadMemberCodeOfConductViewModel>(viewResult.Model);
        Assert.False(model.IsSuccess);
        Assert.Contains("File size exceeds limit", model.Message);
    }

    [Fact]
    public async Task DownloadCodeOfConduct_AsAdmin_ShouldReturnFile()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        SetupAuthenticatedUser(Guid.NewGuid(), isAdmin: true);
        
        var fileStream = new FileStream(Path.GetTempFileName(), FileMode.OpenOrCreate, FileAccess.ReadWrite);
        _memberCocService.GetMemberCodeOfConductAsync(memberId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<FileStream?>(fileStream));
        _memberCocService.GetMemberCodeOfConductFileNameAsync(memberId, Arg.Any<CancellationToken>())
            .Returns("CodeOfConduct.pdf");

        // Act
        var result = await _controller.DownloadCodeOfConduct(memberId, CancellationToken.None);

        // Assert
        var fileResult = Assert.IsType<FileStreamResult>(result);
        Assert.Equal("application/pdf", fileResult.ContentType);
        Assert.Equal("CodeOfConduct.pdf", fileResult.FileDownloadName);
    }

    [Fact]
    public async Task DownloadCodeOfConduct_AsOwnMember_ShouldReturnFile()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        SetupAuthenticatedUser(memberId);
        
        var fileStream = new FileStream(Path.GetTempFileName(), FileMode.OpenOrCreate, FileAccess.ReadWrite);
        _memberCocService.GetMemberCodeOfConductAsync(memberId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<FileStream?>(fileStream));
        _memberCocService.GetMemberCodeOfConductFileNameAsync(memberId, Arg.Any<CancellationToken>())
            .Returns("CodeOfConduct.pdf");

        // Act
        var result = await _controller.DownloadCodeOfConduct(memberId, CancellationToken.None);

        // Assert
        var fileResult = Assert.IsType<FileStreamResult>(result);
        Assert.Equal("application/pdf", fileResult.ContentType);
    }

    [Fact]
    public async Task DownloadCodeOfConduct_AsOtherMember_ShouldReturnForbid()
    {
        // Arrange
        var otherMemberId = Guid.NewGuid();
        SetupAuthenticatedUser(Guid.NewGuid()); // Different member

        // Act
        var result = await _controller.DownloadCodeOfConduct(otherMemberId, CancellationToken.None);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task DownloadCodeOfConduct_WhenFileNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        SetupAuthenticatedUser(memberId);
        
        _memberCocService.GetMemberCodeOfConductAsync(memberId, Arg.Any<CancellationToken>())
            .Returns((FileStream?)null);

        // Act
        var result = await _controller.DownloadCodeOfConduct(memberId, CancellationToken.None);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("not found", notFoundResult.Value?.ToString());
    }

    #endregion

    #region Role Management Tests

    [Fact]
    public async Task UpdateRoles_Get_WithValidMember_ShouldReturnViewWithModel()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var member = new FlowMember
        {
            Id = memberId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            ProfileImageUrl = "/img/profile.jpg"
        };

        _userManager.FindByIdAsync(memberId.ToString()).Returns(member);
        _userManager.GetRolesAsync(member).Returns(new List<string> { "Member" });
        
        // Add roles to the database context for EF Core async support
        var roles = new List<FlowRoles>
        {
            new() { Id = Guid.NewGuid(), Name = "Member", RoleDescription = "Regular member", NormalizedName = "MEMBER" },
            new() { Id = Guid.NewGuid(), Name = "Admin", RoleDescription = "Administrator", NormalizedName = "ADMIN" }
        };
        
        _context.Roles.AddRange(roles);
        await _context.SaveChangesAsync();
        
        _roleManager.Roles.Returns(_context.Roles.AsQueryable());

        // Act
        var result = await _controller.UpdateRoles(memberId, CancellationToken.None);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<UpdateMemberRolesViewModel>(viewResult.Model);
        Assert.Equal(memberId, model.MemberId);
        Assert.Equal("John Doe", model.MemberName);
        Assert.Equal(2, model.AvailableRoles.Count);
        Assert.Single(model.CurrentRoles);
    }

    [Fact]
    public async Task UpdateRoles_Get_WithNonExistentMember_ShouldReturnNotFound()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        _userManager.FindByIdAsync(memberId.ToString()).Returns((FlowMember?)null);

        // Act
        var result = await _controller.UpdateRoles(memberId, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UpdateRoles_Post_WithValidData_ShouldUpdateRolesAndRedirect()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var member = new FlowMember
        {
            Id = memberId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
        };

        var model = new UpdateMemberRolesViewModel
        {
            MemberId = memberId,
            SelectedRoles = new List<string> { "Admin", "Member" }
        };

        _userManager.FindByIdAsync(memberId.ToString()).Returns(member);
        _userManager.GetRolesAsync(member).Returns(new List<string> { "Member" });
        _userManager.AddToRolesAsync(member, Arg.Any<IEnumerable<string>>())
            .Returns(IdentityResult.Success);

        // Act
        var result = await _controller.UpdateRoles(model, CancellationToken.None);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirectResult.ActionName);
        Assert.Equal(memberId, redirectResult.RouteValues?["id"]);
        
        await _userManager.Received(1).AddToRolesAsync(member, Arg.Is<IEnumerable<string>>(
            roles => roles.Contains("Admin")));
    }

    [Fact]
    public async Task UpdateRoles_Post_ShouldRemoveUnselectedRoles()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var member = new FlowMember
        {
            Id = memberId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
        };

        var model = new UpdateMemberRolesViewModel
        {
            MemberId = memberId,
            SelectedRoles = new List<string> { "Member" }
        };

        _userManager.FindByIdAsync(memberId.ToString()).Returns(member);
        _userManager.GetRolesAsync(member).Returns(new List<string> { "Member", "Admin" });
        _userManager.RemoveFromRolesAsync(member, Arg.Any<IEnumerable<string>>())
            .Returns(IdentityResult.Success);

        // Act
        var result = await _controller.UpdateRoles(model, CancellationToken.None);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirectResult.ActionName);
        
        await _userManager.Received(1).RemoveFromRolesAsync(member, Arg.Is<IEnumerable<string>>(
            roles => roles.Contains("Admin")));
    }

    [Fact]
    public async Task UpdateRoles_Post_WhenAddRolesFails_ShouldReturnViewWithErrors()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var member = new FlowMember
        {
            Id = memberId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
        };

        var model = new UpdateMemberRolesViewModel
        {
            MemberId = memberId,
            SelectedRoles = new List<string> { "Admin", "Member" }
        };

        _userManager.FindByIdAsync(memberId.ToString()).Returns(member);
        _userManager.GetRolesAsync(member).Returns(new List<string> { "Member" });
        _userManager.AddToRolesAsync(member, Arg.Any<IEnumerable<string>>())
            .Returns(IdentityResult.Failed(new IdentityError { Description = "Role assignment failed" }));
        
        // Add roles to database for EF Core async support
        var roles = new List<FlowRoles>
        {
            new() { Id = Guid.NewGuid(), Name = "Member", RoleDescription = "Regular member", NormalizedName = "MEMBER" },
            new() { Id = Guid.NewGuid(), Name = "Admin", RoleDescription = "Administrator", NormalizedName = "ADMIN" }
        };
        _context.Roles.AddRange(roles);
        await _context.SaveChangesAsync();
        
        _roleManager.Roles.Returns(_context.Roles.AsQueryable());

        // Act
        var result = await _controller.UpdateRoles(model, CancellationToken.None);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(_controller.ModelState.IsValid);
    }

    #endregion

    #region Index and Details Tests

    [Fact]
    public async Task Index_ShouldReturnViewWithMembers()
    {
        // Arrange
        var members = new List<FlowMember>
        {
            new()
            {
                Id = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                UserName = "johndoe",
                HasUploadedCodeOfConduct = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane@example.com",
                UserName = "janesmith",
                HasUploadedCodeOfConduct = false
            }
        };

        _context.FlowMembers.AddRange(members);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.Index(CancellationToken.None);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<List<FlowMemberDetailsViewModel>>(viewResult.Model);
        Assert.Equal(2, model.Count);
        Assert.Contains(model, m => m.HasUploadedCodeOfConduct);
        Assert.Contains(model, m => !m.HasUploadedCodeOfConduct);
    }

    [Fact]
    public async Task Details_WithValidId_ShouldReturnViewWithMember()
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
            CodeOfConductUploadedAt = DateTime.UtcNow
        };

        _context.FlowMembers.Add(member);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.Details(memberId, CancellationToken.None);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<FlowMemberDetailsViewModel>(viewResult.Model);
        Assert.Equal(memberId, model.Id);
        Assert.Equal("John Doe", model.FullName);
        Assert.True(model.HasUploadedCodeOfConduct);
        Assert.NotNull(model.CodeOfConductUploadedAt);
    }

    [Fact]
    public async Task Details_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var memberId = Guid.NewGuid();

        // Act
        var result = await _controller.Details(memberId, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    #endregion

    #region Edit Member Tests

    [Fact]
    public async Task Edit_Get_AsAdmin_WithValidId_ShouldReturnViewWithModel()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        SetupAuthenticatedUser(Guid.NewGuid(), isAdmin: true);
        
        var member = new FlowMember
        {
            Id = memberId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            UserName = "johndoe",
            DoB = new DateTime(1990, 1, 1),
            WhatsAppNumber = "+234 123 456 7890",
            Bio = "This is a test bio that meets the minimum length requirement for the FlowMember entity.",
            BornAgainDate = new DateTime(2010, 1, 1),
            ProfileImageUrl = "https://example.com/profile.jpg",
            WaterBaptismDate = new DateTime(2011, 1, 1),
            HolySpiritBaptismDate = new DateTime(2012, 1, 1),
            HearsGod = true,
            HowTheyStartedHearingGod = "Through prayer and fasting, I began to hear the voice of God clearly and consistently over a period of time.",
            CoverSpeech = "I joined Flow to serve God and grow spiritually.",
            IsActive = true,
            IsDeleted = false
        };
        
        await _context.FlowMembers.AddAsync(member);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.Edit(memberId, CancellationToken.None);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<EditFlowMemberViewModel>(viewResult.Model);
        Assert.Equal(memberId, model.Id);
        Assert.Equal("John", model.FirstName);
        Assert.Equal("Doe", model.LastName);
        Assert.Equal("john.doe@example.com", model.Email);
    }

    [Fact]
    public async Task Edit_Get_WithDeletedMember_ShouldReturnNotFound()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        SetupAuthenticatedUser(Guid.NewGuid(), isAdmin: true);
        
        var member = new FlowMember
        {
            Id = memberId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            UserName = "johndoe",
            DoB = new DateTime(1990, 1, 1),
            WhatsAppNumber = "+234 123 456 7890",
            Bio = "This is a test bio that meets the minimum length requirement for the FlowMember entity.",
            BornAgainDate = new DateTime(2010, 1, 1),
            ProfileImageUrl = "https://example.com/profile.jpg",
            WaterBaptismDate = new DateTime(2011, 1, 1),
            HolySpiritBaptismDate = new DateTime(2012, 1, 1),
            HearsGod = true,
            HowTheyStartedHearingGod = "Through prayer and fasting, I began to hear the voice of God clearly and consistently over a period of time.",
            IsActive = true,
            IsDeleted = true
        };
        
        await _context.FlowMembers.AddAsync(member);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.Edit(memberId, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Post_AsAdmin_WithValidData_ShouldUpdateMember()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        SetupAuthenticatedUser(Guid.NewGuid(), isAdmin: true);
        
        var member = new FlowMember
        {
            Id = memberId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            UserName = "johndoe",
            DoB = new DateTime(1990, 1, 1),
            WhatsAppNumber = "+234 123 456 7890",
            Bio = "This is a test bio that meets the minimum length requirement for the FlowMember entity.",
            BornAgainDate = new DateTime(2010, 1, 1),
            ProfileImageUrl = "https://example.com/profile.jpg",
            WaterBaptismDate = new DateTime(2011, 1, 1),
            HolySpiritBaptismDate = new DateTime(2012, 1, 1),
            HearsGod = true,
            HowTheyStartedHearingGod = "Through prayer and fasting, I began to hear the voice of God clearly and consistently over a period of time.",
            CoverSpeech = "I joined Flow to serve God.",
            IsActive = true,
            IsDeleted = false
        };
        
        await _context.FlowMembers.AddAsync(member);
        await _context.SaveChangesAsync();

        var viewModel = new EditFlowMemberViewModel
        {
            Id = memberId,
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@example.com",
            DoB = new DateTime(1991, 2, 2),
            WhatsAppNumber = "+234 987 654 3210",
            Bio = "Updated bio with sufficient length to meet requirements for the FlowMember entity validation.",
            BornAgainDate = new DateTime(2011, 2, 2),
            ProfileImageUrl = "https://example.com/newprofile.jpg",
            WaterBaptismDate = new DateTime(2012, 2, 2),
            HolySpiritBaptismDate = new DateTime(2013, 2, 2),
            HearsGod = false,
            HowTheyStartedHearingGod = "Updated testimony about hearing God's voice through scripture reading and meditation on His word over time.",
            CoverSpeech = "Updated cover speech.",
            IsActive = false
        };

        // Act
        var result = await _controller.Edit(viewModel, CancellationToken.None);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirectResult.ActionName);
        
        var updatedMember = await _context.FlowMembers.FindAsync(memberId);
        Assert.NotNull(updatedMember);
        Assert.Equal("Jane", updatedMember.FirstName);
        Assert.Equal("Smith", updatedMember.LastName);
        Assert.False(updatedMember.IsActive);
    }

    [Fact]
    public async Task Edit_Post_AsAdmin_WithCodeOfConductPdf_ShouldUploadAndUpdateMember()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        SetupAuthenticatedUser(Guid.NewGuid(), isAdmin: true);
        
        var member = new FlowMember
        {
            Id = memberId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            UserName = "johndoe",
            DoB = new DateTime(1990, 1, 1),
            WhatsAppNumber = "+234 123 456 7890",
            Bio = "This is a test bio that meets the minimum length requirement for the FlowMember entity.",
            BornAgainDate = new DateTime(2010, 1, 1),
            ProfileImageUrl = "https://example.com/profile.jpg",
            WaterBaptismDate = new DateTime(2011, 1, 1),
            HolySpiritBaptismDate = new DateTime(2012, 1, 1),
            HearsGod = true,
            HowTheyStartedHearingGod = "Through prayer and fasting, I began to hear the voice of God clearly and consistently over a period of time.",
            IsActive = true,
            IsDeleted = false
        };
        
        await _context.FlowMembers.AddAsync(member);
        await _context.SaveChangesAsync();

        var file = CreateMockPdfFile("code-of-conduct.pdf", 1024);
        var filePath = "code-of-conduct/members/test.pdf";
        
        _memberCocService.UploadMemberCodeOfConductAsync(file, memberId, Arg.Any<CancellationToken>())
            .Returns(filePath);

        var viewModel = new EditFlowMemberViewModel
        {
            Id = memberId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            DoB = new DateTime(1990, 1, 1),
            WhatsAppNumber = "+234 123 456 7890",
            Bio = "This is a test bio that meets the minimum length requirement for the FlowMember entity.",
            BornAgainDate = new DateTime(2010, 1, 1),
            ProfileImageUrl = "https://example.com/profile.jpg",
            WaterBaptismDate = new DateTime(2011, 1, 1),
            HolySpiritBaptismDate = new DateTime(2012, 1, 1),
            HearsGod = true,
            HowTheyStartedHearingGod = "Through prayer and fasting, I began to hear the voice of God clearly and consistently over a period of time.",
            IsActive = true,
            CodeOfConductPdf = file
        };

        // Act
        var result = await _controller.Edit(viewModel, CancellationToken.None);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirectResult.ActionName);
        
        var updatedMember = await _context.FlowMembers.FindAsync(memberId);
        Assert.NotNull(updatedMember);
        Assert.True(updatedMember.HasUploadedCodeOfConduct);
        Assert.NotNull(updatedMember.CodeOfConductUploadedAt);
        Assert.Equal(filePath, updatedMember.CodeOfConductPdfPath);
    }

    #endregion

    #region Delete Member Tests

    [Fact]
    public async Task SoftDelete_AsAdmin_WithValidId_ShouldMarkAsDeleted()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        SetupAuthenticatedUser(Guid.NewGuid(), isAdmin: true);
        
        var member = new FlowMember
        {
            Id = memberId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            UserName = "johndoe",
            DoB = new DateTime(1990, 1, 1),
            WhatsAppNumber = "+234 123 456 7890",
            Bio = "This is a test bio that meets the minimum length requirement for the FlowMember entity.",
            BornAgainDate = new DateTime(2010, 1, 1),
            ProfileImageUrl = "https://example.com/profile.jpg",
            WaterBaptismDate = new DateTime(2011, 1, 1),
            HolySpiritBaptismDate = new DateTime(2012, 1, 1),
            HearsGod = true,
            HowTheyStartedHearingGod = "Through prayer and fasting, I began to hear the voice of God clearly and consistently over a period of time.",
            IsActive = true,
            IsDeleted = false
        };
        
        await _context.FlowMembers.AddAsync(member);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.SoftDelete(memberId, CancellationToken.None);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        
        var deletedMember = await _context.FlowMembers.FindAsync(memberId);
        Assert.NotNull(deletedMember);
        Assert.True(deletedMember.IsDeleted);
        Assert.False(deletedMember.IsActive);
    }

    [Fact]
    public async Task SoftDelete_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        SetupAuthenticatedUser(Guid.NewGuid(), isAdmin: true);

        // Act
        var result = await _controller.SoftDelete(memberId, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task HardDelete_AsSuperAdmin_WithValidId_ShouldDeletePermanently()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        SetupAuthenticatedUserWithEmail(Guid.NewGuid(), "josephizang@gmail.com", isAdmin: true);
        
        var member = new FlowMember
        {
            Id = memberId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            UserName = "johndoe",
            DoB = new DateTime(1990, 1, 1),
            WhatsAppNumber = "+234 123 456 7890",
            Bio = "This is a test bio that meets the minimum length requirement for the FlowMember entity.",
            BornAgainDate = new DateTime(2010, 1, 1),
            ProfileImageUrl = "https://example.com/profile.jpg",
            WaterBaptismDate = new DateTime(2011, 1, 1),
            HolySpiritBaptismDate = new DateTime(2012, 1, 1),
            HearsGod = true,
            HowTheyStartedHearingGod = "Through prayer and fasting, I began to hear the voice of God clearly and consistently over a period of time.",
            IsActive = true,
            IsDeleted = false
        };
        
        await _context.FlowMembers.AddAsync(member);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.HardDelete(memberId, CancellationToken.None);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        
        var deletedMember = await _context.FlowMembers.FindAsync(memberId);
        Assert.Null(deletedMember);
    }

    [Fact]
    public async Task HardDelete_AsRegularAdmin_ShouldRedirectWithError()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        SetupAuthenticatedUserWithEmail(Guid.NewGuid(), "admin@example.com", isAdmin: true);
        
        var member = new FlowMember
        {
            Id = memberId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            UserName = "johndoe",
            DoB = new DateTime(1990, 1, 1),
            WhatsAppNumber = "+234 123 456 7890",
            Bio = "This is a test bio that meets the minimum length requirement for the FlowMember entity.",
            BornAgainDate = new DateTime(2010, 1, 1),
            ProfileImageUrl = "https://example.com/profile.jpg",
            WaterBaptismDate = new DateTime(2011, 1, 1),
            HolySpiritBaptismDate = new DateTime(2012, 1, 1),
            HearsGod = true,
            HowTheyStartedHearingGod = "Through prayer and fasting, I began to hear the voice of God clearly and consistently over a period of time.",
            IsActive = true,
            IsDeleted = false
        };
        
        await _context.FlowMembers.AddAsync(member);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.HardDelete(memberId, CancellationToken.None);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirectResult.ActionName);
        Assert.Equal(memberId, redirectResult.RouteValues?["id"]);
        
        var member2 = await _context.FlowMembers.FindAsync(memberId);
        Assert.NotNull(member2); // Member should still exist
    }

    [Fact]
    public async Task HardDelete_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        SetupAuthenticatedUserWithEmail(Guid.NewGuid(), "josephizang@gmail.com", isAdmin: true);

        // Act
        var result = await _controller.HardDelete(memberId, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    #endregion

    #region Helper Methods

    private void SetupAuthenticatedUser(Guid userId, bool isAdmin = false)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, "testuser")
        };

        if (isAdmin)
        {
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));
        }

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            }
        };
    }

    private void SetupAuthenticatedUserWithEmail(Guid userId, string email, bool isAdmin = false)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, "testuser"),
            new(ClaimTypes.Email, email)
        };

        if (isAdmin)
        {
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));
        }

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            }
        };
    }

    private IFormFile CreateMockPdfFile(string fileName, long size)
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

    #endregion

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
