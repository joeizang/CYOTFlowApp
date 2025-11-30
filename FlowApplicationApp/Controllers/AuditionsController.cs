using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FlowApplicationApp.Data;
using FlowApplicationApp.Infrastructure.Extensions;
using FlowApplicationApp.ViewModels.Auditions;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FlowApplicationApp.Controllers
{
    [Route("[controller]")]
    [AutoValidateAntiforgeryToken]
    public sealed class AuditionsController(
        ILogger<AuditionsController> logger,
        IValidator<CreateAuditionerInputModel> validator,
        IWebHostEnvironment hosting,
        ApplicationDbContext context)
        : Controller
    {
        private readonly ILogger<AuditionsController> _logger = logger;
        private readonly IValidator<CreateAuditionerInputModel> _validator = validator;

        [HttpGet]
        public IActionResult Index()
        {
            return View();
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
            // var validationResult = await _validator.ValidateAsync(inputModel);
            // if (!validationResult.IsValid)
            // {
            //     foreach (var error in validationResult.Errors)
            //     {
            //         ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            //     }
            //     return View("Index", inputModel);
            // }
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
        public async Task<IActionResult> Update(Guid id, CancellationToken cancellationToken)
        {
            var auditioner = await context.FlowAuditioners.AsNoTracking()
                .Where(a => a.Id == id)
                .SingleOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
            
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
        public async Task<IActionResult> UpdateAuditioner(Guid id, [Bind]UpdateAuditionerInputModel inputModel, CancellationToken cancellationToken)
        {
            if (id != inputModel.Id) return BadRequest();
            
            if (!ModelState.IsValid) return View("Update", inputModel);
            
            var auditioner = await context.FlowAuditioners
                .Where(a => a.Id == id)
                .SingleOrDefaultAsync(cancellationToken)
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
            auditioner.UpdatedOn = DateOnly.FromDateTime(DateTime.UtcNow);
            
            // Handle profile image upload
            if (inputModel.ProfileImage != null && inputModel.ProfileImage.Length > 0 && inputModel.ProfileImage.Length < 800000)
            {
                var pathForSaving = Path.GetFullPath(Path.Combine(hosting.ContentRootPath, "../../uploadFiles"));
                var uniqueFileName = $"{Guid.NewGuid()}_{inputModel.ProfileImage.FileName}";
                var finalPath = Path.Combine(pathForSaving, uniqueFileName);
                
                using (var stream = new FileStream(finalPath, FileMode.Create))
                {
                    await inputModel.ProfileImage.CopyToAsync(stream, cancellationToken).ConfigureAwait(false);
                }
                
                auditioner.ProfileImageUrl = $"/uploads/{uniqueFileName}";
            }
            
            // Create FlowMember if auditioner is newly accepted into Flow
            if (!wasAccepted && inputModel.AcceptedIntoFlow)
            {
                // Check if member already exists
                var existingMember = await context.FlowMembers
                    .Where(m => m.Email == auditioner.Email)
                    .AnyAsync(cancellationToken)
                    .ConfigureAwait(false);
                
                if (!existingMember)
                {
                    var flowMember = new Data.DomainModels.FlowMember
                    {
                        Id = Guid.NewGuid(),
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
                        CreatedOn = DateOnly.FromDateTime(DateTime.UtcNow),
                        UpdatedOn = DateOnly.FromDateTime(DateTime.UtcNow),
                        IsActive = true,
                        IsDeleted = false
                    };
                    
                    context.FlowMembers.Add(flowMember);
                }
            }
            
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return RedirectToAction("Details", new { id = auditioner.Id });
        }
    }
}