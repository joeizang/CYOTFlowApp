using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace FlowApplicationApp.Data.DomainModels;
public class FlowMember : IdentityUser<Guid>
{       
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string LastName { get; set; } = string.Empty;

        public DateTime DoB { get; set; }
        
        [Required]
        [StringLength(500, MinimumLength = 50)]
        public string Bio { get; set; } = string.Empty;

        public DateTime BornAgainDate { get; set; }
        
        [Required]
        [StringLength(40, MinimumLength = 11)]
        public string WhatsAppNumber { get; set; } = string.Empty;
        
        [Required]
        [StringLength(150, MinimumLength = 10)]
        public string ProfileImageUrl { get; set; } = string.Empty;

        public DateTime WaterBaptismDate { get; set; }

        public DateTime HolySpiritBaptismDate { get; set; }

        public bool HearsGod { get; set; }
        
        [Required]
        [StringLength(500, MinimumLength = 100)]
        public string HowTheyStartedHearingGod { get; set; } = string.Empty;

        public bool ShortlistedForAudition { get; set; }

        public bool AcceptedIntoFlow { get; set; }

        public string CoverSpeech { get; set; } = string.Empty;

        public List<FlowRoles> Roles { get; set; } = [];

        public DateTime CreatedOn { get; set; }

        public DateTime UpdatedOn { get; set; }

        public bool IsActive { get; set; }

        public bool IsDeleted { get; set; }
        
        // Code of Conduct PDF Upload Properties
        public string? CodeOfConductPdfPath { get; set; }
        
        public bool HasUploadedCodeOfConduct { get; set; }
        
        public DateTime? CodeOfConductUploadedAt { get; set; }
}