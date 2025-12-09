using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FlowApplicationApp.Data;
using FlowApplicationApp.Data.DomainModels;
using FlowApplicationApp.Infrastructure.Services;
using FlowApplicationApp.ViewModels.FlowMembers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FlowApplicationApp.Controllers
{
    [Route("[controller]")]
    [Authorize(Roles = "Admin")]
    [AutoValidateAntiforgeryToken]
    public sealed class FlowMembersController : Controller
    {
        private readonly ILogger<FlowMembersController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IMemberCodeOfConductService _memberCocService;
        private readonly UserManager<FlowMember> _userManager;
        private readonly RoleManager<FlowRoles> _roleManager;

        public FlowMembersController(
            ILogger<FlowMembersController> logger,
            ApplicationDbContext context,
            IMemberCodeOfConductService memberCocService,
            UserManager<FlowMember> userManager,
            RoleManager<FlowRoles> roleManager
        )
        {
            _logger = logger;
            _context = context;
            _memberCocService = memberCocService;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var members = await _context.FlowMembers.AsNoTracking()
                .Select(m => new FlowMemberDetailsViewModel
                {
                    Id = m.Id,
                    FullName = $"{m.FirstName} {m.LastName}",
                    Email = m.Email ?? string.Empty,
                    UserName = m.UserName ?? string.Empty,
                    ProfileImageUrl = m.ProfileImageUrl,
                    Bio = m.Bio,
                    IsActive = m.IsActive,
                    HasUploadedCodeOfConduct = m.HasUploadedCodeOfConduct,
                    CodeOfConductUploadedAt = m.CodeOfConductUploadedAt,
                    Roles = m.Roles.Select(r => r.Name ?? string.Empty).ToList()
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
                
            return View(members);
        }

        [HttpGet("details/{id:guid}")]
        public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
        {
            var member = await _context.FlowMembers.AsNoTracking()
                .Where(m => m.Id == id)
                .Select(m => new FlowMemberDetailsViewModel
                {
                    Id = m.Id,
                    FullName = $"{m.FirstName} {m.LastName}",
                    Email = m.Email ?? string.Empty,
                    UserName = m.UserName ?? string.Empty,
                    DoB = m.DoB,
                    WhatsAppNumber = m.WhatsAppNumber,
                    Bio = m.Bio,
                    BornAgainDate = m.BornAgainDate,
                    ProfileImageUrl = m.ProfileImageUrl,
                    WaterBaptismDate = m.WaterBaptismDate,
                    HolySpiritBaptismDate = m.HolySpiritBaptismDate,
                    HearsGod = m.HearsGod,
                    HowTheyStartedHearingGod = m.HowTheyStartedHearingGod,
                    CoverSpeech = m.CoverSpeech,
                    CreatedOn = m.CreatedOn,
                    UpdatedOn = m.UpdatedOn,
                    IsActive = m.IsActive,
                    HasUploadedCodeOfConduct = m.HasUploadedCodeOfConduct,
                    CodeOfConductUploadedAt = m.CodeOfConductUploadedAt,
                    CodeOfConductPdfPath = m.CodeOfConductPdfPath,
                    Roles = m.Roles.Select(r => r.Name ?? string.Empty).ToList()
                })
                .SingleOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
                
            if (member == null) return NotFound();
            
            return View(member);
        }

        // Code of Conduct PDF Upload Actions
        
        [HttpGet("upload-code-of-conduct")]
        [AllowAnonymous]
        public async Task<IActionResult> UploadCodeOfConduct(CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var memberId = Guid.Parse(userId);
            var hasUploaded = await _memberCocService.HasUploadedCodeOfConductAsync(memberId, cancellationToken);
            
            var viewModel = new UploadMemberCodeOfConductViewModel();
            
            if (hasUploaded)
            {
                var fileName = await _memberCocService.GetMemberCodeOfConductFileNameAsync(memberId, cancellationToken);
                viewModel.Message = "You have already uploaded your Code of Conduct. You can upload a new version to replace it.";
                viewModel.DownloadUrl = Url.Action("DownloadCodeOfConduct", "FlowMembers", new { id = memberId });
            }

            return View(viewModel);
        }

        [HttpPost("upload-code-of-conduct")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadCodeOfConduct(IFormFile? file, CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var memberId = Guid.Parse(userId);

            if (file == null || file.Length == 0)
            {
                var errorModel = new UploadMemberCodeOfConductViewModel
                {
                    IsSuccess = false,
                    Message = "Please select a PDF file to upload."
                };
                return View(errorModel);
            }

            try
            {
                var filePath = await _memberCocService.UploadMemberCodeOfConductAsync(file, memberId, cancellationToken);

                var successModel = new UploadMemberCodeOfConductViewModel
                {
                    IsSuccess = true,
                    Message = "Your Code of Conduct has been uploaded successfully! Your account is now active.",
                    DownloadUrl = Url.Action("DownloadCodeOfConduct", "FlowMembers", new { id = memberId }),
                    UploadedAt = DateTime.UtcNow
                };

                return View(successModel);
            }
            catch (InvalidOperationException ex)
            {
                var errorModel = new UploadMemberCodeOfConductViewModel
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
                return View(errorModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading Code of Conduct for member {MemberId}", memberId);
                var errorModel = new UploadMemberCodeOfConductViewModel
                {
                    IsSuccess = false,
                    Message = "An error occurred while uploading your file. Please try again."
                };
                return View(errorModel);
            }
        }

        [HttpGet("download-code-of-conduct/{id:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> DownloadCodeOfConduct(Guid id, CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");
            
            // Only allow admins or the member themselves to download
            if (!isAdmin && userId != id.ToString())
            {
                return Forbid();
            }

            var fileStream = await _memberCocService.GetMemberCodeOfConductAsync(id, cancellationToken);
            
            if (fileStream == null)
            {
                return NotFound("Code of Conduct document not found.");
            }

            var fileName = await _memberCocService.GetMemberCodeOfConductFileNameAsync(id, cancellationToken);
            
            return File(fileStream, "application/pdf", fileName ?? "CodeOfConduct.pdf");
        }

        // Role Management Actions
        
        [HttpGet("update-roles/{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateRoles(Guid id, CancellationToken cancellationToken)
        {
            var member = await _userManager.FindByIdAsync(id.ToString());
            if (member == null)
            {
                return NotFound();
            }

            var currentRoles = await _userManager.GetRolesAsync(member);
            var allRoles = await _roleManager.Roles
                .Select(r => new RoleSelectionItem
                {
                    RoleName = r.Name ?? string.Empty,
                    RoleDescription = r.RoleDescription,
                    IsSelected = currentRoles.Contains(r.Name ?? string.Empty)
                })
                .ToListAsync(cancellationToken);

            var viewModel = new UpdateMemberRolesViewModel
            {
                MemberId = member.Id,
                MemberName = $"{member.FirstName} {member.LastName}",
                Email = member.Email ?? string.Empty,
                ProfileImageUrl = member.ProfileImageUrl,
                CurrentRoles = currentRoles.ToList(),
                AvailableRoles = allRoles
            };

            return View(viewModel);
        }

        [HttpPost("update-roles/{id:guid?}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateRoles(UpdateMemberRolesViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                // Reload roles if validation fails
                var allRoles = await _roleManager.Roles
                    .Select(r => new RoleSelectionItem
                    {
                        RoleName = r.Name ?? string.Empty,
                        RoleDescription = r.RoleDescription,
                        IsSelected = model.SelectedRoles.Contains(r.Name ?? string.Empty)
                    })
                    .ToListAsync(cancellationToken);
                
                model.AvailableRoles = allRoles;
                return View(model);
            }

            var member = await _userManager.FindByIdAsync(model.MemberId.ToString());
            if (member == null)
            {
                return NotFound();
            }

            try
            {
                var currentRoles = await _userManager.GetRolesAsync(member);
                
                // Determine roles to add and remove
                var rolesToAdd = model.SelectedRoles.Except(currentRoles).ToList();
                var rolesToRemove = currentRoles.Except(model.SelectedRoles).ToList();

                // Add new roles
                if (rolesToAdd.Any())
                {
                    var addResult = await _userManager.AddToRolesAsync(member, rolesToAdd);
                    if (!addResult.Succeeded)
                    {
                        foreach (var error in addResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        
                        var allRoles = await _roleManager.Roles
                            .Select(r => new RoleSelectionItem
                            {
                                RoleName = r.Name ?? string.Empty,
                                RoleDescription = r.RoleDescription,
                                IsSelected = model.SelectedRoles.Contains(r.Name ?? string.Empty)
                            })
                            .ToListAsync(cancellationToken);
                        
                        model.AvailableRoles = allRoles;
                        return View(model);
                    }
                }

                // Remove old roles
                if (rolesToRemove.Any())
                {
                    var removeResult = await _userManager.RemoveFromRolesAsync(member, rolesToRemove);
                    if (!removeResult.Succeeded)
                    {
                        foreach (var error in removeResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        
                        var allRoles = await _roleManager.Roles
                            .Select(r => new RoleSelectionItem
                            {
                                RoleName = r.Name ?? string.Empty,
                                RoleDescription = r.RoleDescription,
                                IsSelected = model.SelectedRoles.Contains(r.Name ?? string.Empty)
                            })
                            .ToListAsync(cancellationToken);
                        
                        model.AvailableRoles = allRoles;
                        return View(model);
                    }
                }

                TempData["SuccessMessage"] = $"Roles updated successfully for {member.FirstName} {member.LastName}";
                return RedirectToAction("Details", new { id = member.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating roles for member {MemberId}", model.MemberId);
                ModelState.AddModelError(string.Empty, "An error occurred while updating roles. Please try again.");
                
                var allRoles = await _roleManager.Roles
                    .Select(r => new RoleSelectionItem
                    {
                        RoleName = r.Name ?? string.Empty,
                        RoleDescription = r.RoleDescription,
                        IsSelected = model.SelectedRoles.Contains(r.Name ?? string.Empty)
                    })
                    .ToListAsync(cancellationToken);
                
                model.AvailableRoles = allRoles;
                return View(model);
            }
        }
        
        // Edit Member Actions
        
        [HttpGet("edit/{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
        {
            var member = await _context.FlowMembers.AsNoTracking()
                .Where(m => m.Id == id && !m.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);
                
            if (member == null)
            {
                return NotFound();
            }

            var viewModel = new EditFlowMemberViewModel
            {
                Id = member.Id,
                FirstName = member.FirstName,
                LastName = member.LastName,
                Email = member.Email ?? string.Empty,
                DoB = member.DoB,
                WhatsAppNumber = member.WhatsAppNumber,
                Bio = member.Bio,
                BornAgainDate = member.BornAgainDate,
                ProfileImageUrl = member.ProfileImageUrl,
                WaterBaptismDate = member.WaterBaptismDate,
                HolySpiritBaptismDate = member.HolySpiritBaptismDate,
                HearsGod = member.HearsGod,
                HowTheyStartedHearingGod = member.HowTheyStartedHearingGod,
                CoverSpeech = member.CoverSpeech,
                IsActive = member.IsActive,
                HasUploadedCodeOfConduct = member.HasUploadedCodeOfConduct,
                CodeOfConductUploadedAt = member.CodeOfConductUploadedAt,
                CurrentCodeOfConductPdfPath = member.CodeOfConductPdfPath
            };

            return View(viewModel);
        }

        [HttpPost("edit/{id:guid?}")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditFlowMemberViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var member = await _context.FlowMembers
                .Where(m => m.Id == model.Id && !m.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);
                
            if (member == null)
            {
                return NotFound();
            }

            try
            {
                // Update member properties
                member.FirstName = model.FirstName;
                member.LastName = model.LastName;
                member.Email = model.Email;
                member.DoB = model.DoB;
                member.WhatsAppNumber = model.WhatsAppNumber;
                member.Bio = model.Bio;
                member.BornAgainDate = model.BornAgainDate;
                member.ProfileImageUrl = model.ProfileImageUrl;
                member.WaterBaptismDate = model.WaterBaptismDate;
                member.HolySpiritBaptismDate = model.HolySpiritBaptismDate;
                member.HearsGod = model.HearsGod;
                member.HowTheyStartedHearingGod = model.HowTheyStartedHearingGod;
                member.CoverSpeech = model.CoverSpeech;
                member.IsActive = model.IsActive;
                member.UpdatedOn = DateTime.UtcNow;

                // Handle Code of Conduct PDF upload if provided
                if (model.CodeOfConductPdf != null && model.CodeOfConductPdf.Length > 0)
                {
                    var filePath = await _memberCocService.UploadMemberCodeOfConductAsync(
                        model.CodeOfConductPdf, 
                        member.Id, 
                        cancellationToken);
                    
                    member.CodeOfConductPdfPath = filePath;
                    member.HasUploadedCodeOfConduct = true;
                    member.CodeOfConductUploadedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync(cancellationToken);
                
                TempData["SuccessMessage"] = $"Successfully updated {member.FirstName} {member.LastName}'s profile.";
                return RedirectToAction("Details", new { id = member.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating member {MemberId}", model.Id);
                ModelState.AddModelError(string.Empty, "An error occurred while updating the member. Please try again.");
                return View(model);
            }
        }
        
        // Delete Member Actions
        
        [HttpPost("soft-delete/{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SoftDelete(Guid id, CancellationToken cancellationToken)
        {
            var member = await _context.FlowMembers
                .Where(m => m.Id == id && !m.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);
                
            if (member == null)
            {
                return NotFound();
            }

            try
            {
                member.IsDeleted = true;
                member.IsActive = false;
                member.UpdatedOn = DateTime.UtcNow;
                
                // Deactivate associated auditions
                var auditions = await _context.FlowAuditioners
                    .Where(a => a.Email == member.Email)
                    .ToListAsync(cancellationToken);
                
                foreach (var audition in auditions)
                {
                    audition.IsActive = false;
                    audition.UpdatedOn = DateTime.UtcNow;
                }
                
                await _context.SaveChangesAsync(cancellationToken);
                
                TempData["SuccessMessage"] = $"Successfully deactivated {member.FirstName} {member.LastName}.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error soft deleting member {MemberId}", id);
                TempData["ErrorMessage"] = "An error occurred while deactivating the member. Please try again.";
                return RedirectToAction("Details", new { id });
            }
        }

        [HttpPost("hard-delete/{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HardDelete(Guid id, CancellationToken cancellationToken)
        {
            // Only allow super admin to hard delete
            var currentUserEmail = User.FindFirstValue(ClaimTypes.Email);
            if (currentUserEmail != "josephizang@gmail.com")
            {
                TempData["ErrorMessage"] = "You do not have permission to perform this action.";
                return RedirectToAction("Details", new { id });
            }

            var member = await _context.FlowMembers
                .Where(m => m.Id == id)
                .FirstOrDefaultAsync(cancellationToken);
                
            if (member == null)
            {
                return NotFound();
            }

            try
            {
                // Deactivate associated auditions
                var auditions = await _context.FlowAuditioners
                    .Where(a => a.Email == member.Email)
                    .ToListAsync(cancellationToken);
                
                foreach (var audition in auditions)
                {
                    audition.IsActive = false;
                    audition.UpdatedOn = DateTime.UtcNow;
                }
                
                // Delete Code of Conduct file if exists
                if (!string.IsNullOrEmpty(member.CodeOfConductPdfPath))
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", member.CodeOfConductPdfPath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.FlowMembers.Remove(member);
                await _context.SaveChangesAsync(cancellationToken);
                
                TempData["SuccessMessage"] = $"Successfully permanently deleted {member.FirstName} {member.LastName}.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error hard deleting member {MemberId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the member. Please try again.";
                return RedirectToAction("Details", new { id });
            }
        }
    }
}
