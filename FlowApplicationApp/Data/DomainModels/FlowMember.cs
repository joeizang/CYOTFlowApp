using Microsoft.AspNetCore.Identity;

namespace FlowApplicationApp.Data.DomainModels;
public class FlowMember : IdentityUser<Guid>
{
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public DateOnly DoB { get; set; }

        public string Bio { get; set; } = string.Empty;

        public DateOnly BornAgainDate { get; set; }

        public string ProfileImageUrl { get; set; } = string.Empty;

        public DateOnly WaterBaptismDate { get; set; }

        public DateOnly HolySpiritBaptismDate { get; set; }

        public bool HearsGod { get; set; }

        public string HowTheyStartedHearingGod { get; set; } = string.Empty;

        public bool ShortlistedForAudition { get; set; }

        public bool AcceptedIntoFlow { get; set; }

        public string CoverSpeech { get; set; } = string.Empty;

        public List<FlowRoles> Roles { get; set; } = [];

        public DateOnly CreatedOn { get; set; }

        public DateOnly UpdatedOn { get; set; }

        public bool IsActive { get; set; }

        public bool IsDeleted { get; set; }
}