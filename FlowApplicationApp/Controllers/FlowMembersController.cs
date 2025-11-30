using System;
using System.Linq;
using System.Threading.Tasks;
using FlowApplicationApp.Data;
using FlowApplicationApp.ViewModels.FlowMembers;
using Microsoft.AspNetCore.Authorization;
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

        public FlowMembersController(
            ILogger<FlowMembersController> logger,
            ApplicationDbContext context
        )
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
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
                    Roles = m.Roles.Select(r => r.Name ?? string.Empty).ToList()
                })
                .SingleOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
                
            if (member == null) return NotFound();
            
            return View(member);
        }
    }
}
