using FlowApplicationApp.Data;
using FlowApplicationApp.Data.DomainModels;
using FlowApplicationApp.Infrastructure.Extensions;
using FlowApplicationApp.Infrastructure.Services;
using FlowApplicationApp.ViewModels.Auditions;
using FlowApplicationApp.ViewModels.FlowMembers;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlowApplicationApp.Controllers
{
    [Route("[controller]")]
    [AutoValidateAntiforgeryToken]
    public sealed class AuditionsController(
        ILogger<AuditionsController> logger,
        IValidator<CreateAuditionerInputModel> validator,
        IWebHostEnvironment hosting,
        ApplicationDbContext context,
        UserManager<FlowMember> userManager,
        RoleManager<FlowRoles> roleManager,
        IEmailSender emailSender)
        : Controller
    {
        private readonly ILogger<AuditionsController> _logger = logger;
        private readonly IValidator<CreateAuditionerInputModel> _validator = validator;

        [HttpGet]
        public IActionResult Index()
        {
            var inputModel = new CreateAuditionerInputModel();
            return View(inputModel);
        }

        [HttpGet("list")]
        public async Task<IActionResult> List(CancellationToken cancellationToken)
        {
            var auditioners = await context.FlowAuditioners.AsNoTracking()
                .Select(a => new AuditionerDetailsViewModel
                {
                    Id = a.Id,
                    FullName = $"{a.FirstName} {a.LastName}",
                    Email = a.Email,
                    DoB = a.DoB,
                    WhatsAppNumber = a.WhatsAppNumber,
                    Bio = a.Bio,
                    BornAgainDate = a.BornAgainDate,
                    ProfileImageUrl = a.ProfileImageUrl,
                    WaterBaptismDate = a.WaterBaptismDate,
                    HolySpiritBaptismDate = a.HolySpiritBaptismDate,
                    HearsGod = a.HearsGod,
                    HowTheyStartedHearingGod = a.HowTheyStartedHearingGod,
                    ShortlistedForAudition = a.ShortlistedForAudition,
                    AuditionDate = a.AuditionDate,
                    AuditionTime = a.AuditionTime,
                    AcceptedIntoFlow = a.AcceptedIntoFlow,
                    CoverSpeech = a.CoverSpeech,
                    IsActive = a.IsActive
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
                
            return View(auditioners);
        }

        [HttpGet("details/{id:guid}")]
        public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
        {
            var auditioner = await context.FlowAuditioners.AsNoTracking()
                .Where(a => a.Id == id)
                .Select(a => new AuditionerDetailsViewModel
                {
                    Id = a.Id,
                    FullName = $"{a.FirstName} {a.LastName}",
                    Email = a.Email,
                    DoB = a.DoB,
                    WhatsAppNumber = a.WhatsAppNumber,
                    Bio = a.Bio,
                    BornAgainDate = a.BornAgainDate,
                    ProfileImageUrl = a.ProfileImageUrl,
                    WaterBaptismDate = a.WaterBaptismDate,
                    HolySpiritBaptismDate = a.HolySpiritBaptismDate,
                    HearsGod = a.HearsGod,
                    HowTheyStartedHearingGod = a.HowTheyStartedHearingGod,
                    ShortlistedForAudition = a.ShortlistedForAudition,
                    AuditionDate = a.AuditionDate,
                    AuditionTime = a.AuditionTime,
                    AcceptedIntoFlow = a.AcceptedIntoFlow,
                    CoverSpeech = a.CoverSpeech,
                    IsActive = a.IsActive
                })
                .SingleOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
            if (auditioner == null) return NotFound();
            return View(auditioner);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAuditioner([Bind]CreateAuditionerInputModel inputModel, CancellationToken cancellationToken)
        {
            Console.WriteLine(inputModel.ProfileImage.FileName);
            if (!ModelState.IsValid) return View("Index", inputModel);
            var validationResult = await _validator.ValidateAsync(inputModel, cancellationToken);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
                return View("Index", inputModel);
            }
            //save every image to the img folder and save the path to the database
            var entity = inputModel.MapToDomainModel();
            if (inputModel.ProfileImage != null && (inputModel.ProfileImage.Length > 0 
                                                    && inputModel.ProfileImage.Length <800000))
            {
                var pathForSaving = Path.GetFullPath(Path.Combine(hosting.ContentRootPath, "../../uploadFiles"));
                var uniqueFileName = inputModel.GetProfileImageFileName();
                var finalPath = Path.Combine(pathForSaving, uniqueFileName);
                using (var stream = new FileStream(finalPath, FileMode.Create))
                {
                    await inputModel.ProfileImage.CopyToAsync(stream, cancellationToken).ConfigureAwait(false);
                }
                entity.ProfileImageUrl = $"/uploads/{uniqueFileName}";
            }
            context.FlowAuditioners.Add(entity);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return RedirectToAction("Details", new { id = entity.Id });
        }

        [HttpGet("update/{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, CancellationToken token)
        {
            var auditioner = await context.FlowAuditioners.FindAsync([id, token], cancellationToken: token).ConfigureAwait(false);
            
            if (auditioner == null) return NotFound();
            
            var inputModel = new UpdateAuditionerInputModel
            {
                Id = auditioner.Id,
                FirstName = auditioner.FirstName,
                LastName = auditioner.LastName,
                Email = auditioner.Email,
                WhatsAppNumber = auditioner.WhatsAppNumber,
                DoB = auditioner.DoB,
                Bio = auditioner.Bio,
                BornAgainDate = auditioner.BornAgainDate,
                WaterBaptismDate = auditioner.WaterBaptismDate,
                HolySpiritBaptismDate = auditioner.HolySpiritBaptismDate,
                HearsGod = auditioner.HearsGod,
                HowTheyStartedHearingGod = auditioner.HowTheyStartedHearingGod,
                CoverSpeech = auditioner.CoverSpeech,
                ShortlistedForAudition = auditioner.ShortlistedForAudition,
                AuditionDate = auditioner.AuditionDate == default ? null : auditioner.AuditionDate,
                AuditionTime = auditioner.AuditionTime == default ? null : auditioner.AuditionTime,
                AcceptedIntoFlow = auditioner.AcceptedIntoFlow,
                IsActive = auditioner.IsActive
            };
            
            return View(inputModel);
        }

        [HttpPost("update/{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id,UpdateAuditionerInputModel inputModel, CancellationToken token)
        {
            if (id != inputModel.Id) return BadRequest();
            
            if (!ModelState.IsValid) return View("Update", inputModel);
            
            var auditioner = await context.FlowAuditioners
                .FindAsync([id, token], cancellationToken: token)
                .ConfigureAwait(false);

            
            if (auditioner == null) return NotFound();
            
            // Update basic properties
            auditioner.FirstName = inputModel.FirstName;
            auditioner.LastName = inputModel.LastName;
            auditioner.Email = inputModel.Email;
            auditioner.WhatsAppNumber = inputModel.WhatsAppNumber;
            auditioner.DoB = inputModel.DoB;
            auditioner.Bio = inputModel.Bio;
            auditioner.BornAgainDate = inputModel.BornAgainDate;
            auditioner.WaterBaptismDate = inputModel.WaterBaptismDate;
            auditioner.HolySpiritBaptismDate = inputModel.HolySpiritBaptismDate;
            auditioner.HearsGod = inputModel.HearsGod;
            auditioner.HowTheyStartedHearingGod = inputModel.HowTheyStartedHearingGod;
            auditioner.CoverSpeech = inputModel.CoverSpeech;
            auditioner.ShortlistedForAudition = inputModel.ShortlistedForAudition;
            auditioner.AuditionDate = inputModel.AuditionDate ?? default;
            auditioner.AuditionTime = inputModel.AuditionTime ?? default;
            
            // Check if auditioner is being accepted into Flow
            var wasAccepted = auditioner.AcceptedIntoFlow;
            auditioner.AcceptedIntoFlow = inputModel.AcceptedIntoFlow;
            auditioner.IsActive = inputModel.IsActive;
            auditioner.UpdatedOn = DateTime.UtcNow;
            
            // Handle profile image upload
            if (inputModel.ProfileImage != null && inputModel.ProfileImage.Length > 0 && inputModel.ProfileImage.Length < 800000)
            {
                var pathForSaving = Path.GetFullPath(Path.Combine(hosting.ContentRootPath, "../../uploadFiles"));
                var uniqueFileName = $"{Guid.NewGuid()}_{inputModel.ProfileImage.FileName}";
                var finalPath = Path.Combine(pathForSaving, uniqueFileName);

                await using (var stream = new FileStream(finalPath, FileMode.Create))
                {
                    await inputModel.ProfileImage.CopyToAsync(stream, token).ConfigureAwait(false);
                }
                
                auditioner.ProfileImageUrl = $"/uploads/{uniqueFileName}";
            }
            
            // Create FlowMember if auditioner is newly accepted into Flow
            if (!wasAccepted && inputModel.AcceptedIntoFlow)
            {
                // Send congratulations email to auditioner
                // var emailSent = await emailSender.SendAuditionPassedEmailAsync(
                //     auditioner.Email,
                //     $"{auditioner.FirstName} {auditioner.LastName}",
                //     token
                // ).ConfigureAwait(false);
                
                // if (emailSent)
                // {
                //     _logger.LogInformation(
                //         "Audition passed email sent successfully to {Email} for auditioner {AuditionerId}",
                //         auditioner.Email,
                //         auditioner.Id
                //     );
                // }
                // else
                // {
                //     _logger.LogWarning(
                //         "Failed to send audition passed email to {Email} for auditioner {AuditionerId}",
                //         auditioner.Email,
                //         auditioner.Id
                //     );
                // }
                
                // Check if member already exists
                var existingMember = await context.FlowMembers
                    .Where(m => m.Email == auditioner.Email)
                    .AnyAsync(token)
                    .ConfigureAwait(false);
                
                if (!existingMember)
                {
                    var flowMember = new Data.DomainModels.FlowMember
                    {
                        Id = Ulid.NewUlid().ToGuid(),
                        FirstName = auditioner.FirstName,
                        LastName = auditioner.LastName,
                        Email = auditioner.Email,
                        UserName = auditioner.Email,
                        DoB = auditioner.DoB,
                        WhatsAppNumber = auditioner.WhatsAppNumber,
                        Bio = auditioner.Bio,
                        BornAgainDate = auditioner.BornAgainDate,
                        ProfileImageUrl = auditioner.ProfileImageUrl,
                        WaterBaptismDate = auditioner.WaterBaptismDate,
                        HolySpiritBaptismDate = auditioner.HolySpiritBaptismDate,
                        HearsGod = auditioner.HearsGod,
                        HowTheyStartedHearingGod = auditioner.HowTheyStartedHearingGod,
                        CoverSpeech = auditioner.CoverSpeech,
                        AcceptedIntoFlow = true,
                        CreatedOn = DateTime.UtcNow,
                        UpdatedOn = DateTime.UtcNow,
                        IsActive = true,
                        IsDeleted = false
                    };
                    
                    // context.FlowMembers.Add(flowMember);
                    var identityResult = await userManager.CreateAsync(flowMember, "Welcome@123").ConfigureAwait(false);
                    if (identityResult.Succeeded)
                    {
                        _logger.LogInformation(
                            "FlowMember created successfully for auditioner {AuditionerId} with email {Email}",
                            auditioner.Id,
                            auditioner.Email
                        );
                        // Assign "Member" role
                        await userManager.AddToRolesAsync(flowMember, ["Member", "Vocalist"]).ConfigureAwait(false);
                    }
                }
            }
            
            await context.SaveChangesAsync(token).ConfigureAwait(false);
            return RedirectToAction("Details", new { id = auditioner.Id });
        }

        [HttpPost("update-member-roles")]
        public async Task<IActionResult> UpdateFlowMemberRoles(UpdateMemberRolesInputModel rolesInputModel, CancellationToken token)
        {
            var member = await userManager.FindByIdAsync(rolesInputModel.MemberId.ToString()).ConfigureAwait(false);
            if (member == null) return NotFound();

            var currentRoles = await userManager.GetRolesAsync(member).ConfigureAwait(false);
            var rolesToAdd = rolesInputModel.Roles.Except(currentRoles).ToList();
            var rolesToRemove = currentRoles.Except(rolesInputModel.Roles).ToList();

            if (rolesToAdd.Count != 0)
            {
                await userManager.AddToRolesAsync(member, rolesToAdd).ConfigureAwait(false);
            }

            if (rolesToRemove.Count != 0)
            {
                await userManager.RemoveFromRolesAsync(member, rolesToRemove).ConfigureAwait(false);
            }

            return RedirectToAction("Details", "FlowMembers", new { id = rolesInputModel.MemberId });
        }
    }
}