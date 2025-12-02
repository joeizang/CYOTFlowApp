using FlowApplicationApp.Infrastructure.Services;
using FlowApplicationApp.ViewModels.CodeOfConduct;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FlowApplicationApp.Data.DomainModels;

namespace FlowApplicationApp.Controllers;

[Route("code-of-conduct")]
public class CodeOfConductController : Controller
{
    private readonly ICodeOfConductService _codeOfConductService;
    private readonly UserManager<FlowMember> _userManager;
    private readonly ILogger<CodeOfConductController> _logger;

    public CodeOfConductController(
        ICodeOfConductService codeOfConductService,
        UserManager<FlowMember> userManager,
        ILogger<CodeOfConductController> logger)
    {
        _codeOfConductService = codeOfConductService;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Display the active Code of Conduct - Public access
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var document = await _codeOfConductService.GetActiveCodeOfConductAsync();
        
        var isAdmin = User.Identity?.IsAuthenticated == true && 
                      User.IsInRole("Admin");

        var viewModel = new CodeOfConductViewModel
        {
            HtmlContent = document?.HtmlContent ?? "<p>No Code of Conduct has been uploaded yet.</p>",
            UploadedAt = document?.UploadedAt,
            Version = document?.Version ?? 0,
            CanEdit = isAdmin,
            DocumentId = document?.Id,
            UploadedByName = document?.UploadedByMember?.UserName
        };

        return View(viewModel);
    }

    /// <summary>
    /// Show upload form - Admin only
    /// </summary>
    [HttpGet("upload")]
    [Authorize(Roles = "Admin")]
    public IActionResult Upload()
    {
        return View(new UploadCodeOfConductViewModel());
    }

    /// <summary>
    /// Handle file upload - Admin only
    /// </summary>
    [HttpPost("upload")]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            var errorModel = new UploadCodeOfConductViewModel
            {
                IsSuccess = false,
                Message = "Please select a file to upload."
            };
            return View(errorModel);
        }

        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var document = await _codeOfConductService.UploadCodeOfConductAsync(file, user.Id);

            var successModel = new UploadCodeOfConductViewModel
            {
                IsSuccess = true,
                Message = $"Code of Conduct version {document.Version} uploaded successfully!",
                NewVersion = document.Version
            };

            return View(successModel);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation during Code of Conduct upload");
            var errorModel = new UploadCodeOfConductViewModel
            {
                IsSuccess = false,
                Message = ex.Message
            };
            return View(errorModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading Code of Conduct");
            var errorModel = new UploadCodeOfConductViewModel
            {
                IsSuccess = false,
                Message = "An error occurred while uploading the document. Please try again."
            };
            return View(errorModel);
        }
    }

    /// <summary>
    /// Download the original DOCX file - Public access
    /// </summary>
    [HttpGet("download")]
    [AllowAnonymous]
    public async Task<IActionResult> Download()
    {
        try
        {
            var document = await _codeOfConductService.GetActiveCodeOfConductAsync();
            
            if (document == null)
            {
                return NotFound("No Code of Conduct document available for download.");
            }

            var fileStream = await _codeOfConductService.GetOriginalDocumentAsync(document.Id);
            
            return File(fileStream, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", document.FileName);
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError(ex, "File not found during download");
            return NotFound("Document file not found.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading Code of Conduct");
            return StatusCode(500, "An error occurred while downloading the document.");
        }
    }

    /// <summary>
    /// Download a specific version - Public access
    /// </summary>
    [HttpGet("download/{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> DownloadVersion(Guid id)
    {
        try
        {
            var versions = await _codeOfConductService.GetAllVersionsAsync();
            var document = versions.FirstOrDefault(v => v.Id == id);

            if (document == null)
            {
                return NotFound("Document not found.");
            }

            var fileStream = await _codeOfConductService.GetOriginalDocumentAsync(id);
            
            return File(fileStream, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", document.FileName);
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError(ex, "File not found during version download");
            return NotFound("Document file not found.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading Code of Conduct version");
            return StatusCode(500, "An error occurred while downloading the document.");
        }
    }

    /// <summary>
    /// View all versions - Admin only
    /// </summary>
    [HttpGet("versions")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Versions()
    {
        var versions = await _codeOfConductService.GetAllVersionsAsync();
        var activeVersion = versions.FirstOrDefault(v => v.IsActive);

        var viewModel = new ManageVersionsViewModel
        {
            Versions = versions,
            ActiveVersionId = activeVersion?.Id
        };

        return View(viewModel);
    }

    /// <summary>
    /// Activate a specific version - Admin only
    /// </summary>
    [HttpPost("activate/{id}")]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(Guid id)
    {
        try
        {
            var success = await _codeOfConductService.SetActiveVersionAsync(id);
            
            if (success)
            {
                TempData["SuccessMessage"] = "Version activated successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to activate version.";
            }

            return RedirectToAction(nameof(Versions));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating Code of Conduct version");
            TempData["ErrorMessage"] = "An error occurred while activating the version.";
            return RedirectToAction(nameof(Versions));
        }
    }
}