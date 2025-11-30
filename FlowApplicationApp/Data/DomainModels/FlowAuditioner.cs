using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlowApplicationApp.Data.DomainModels;
public class FlowAuditioner
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public DateOnly DoB { get; set; }

    public string WhatsAppNumber { get; set; } = string.Empty;

    public string Bio { get; set; } = string.Empty;

    public DateOnly BornAgainDate { get; set; }

    public string ProfileImageUrl { get; set; } = string.Empty;

    public DateOnly WaterBaptismDate { get; set; }

    public DateOnly HolySpiritBaptismDate { get; set; }

    public bool HearsGod { get; set; }

    public string HowTheyStartedHearingGod { get; set; } = string.Empty;

    public bool ShortlistedForAudition { get; set; }

    public DateOnly AuditionDate { get; set; }

    public TimeOnly AuditionTime { get; set; }

    public bool AcceptedIntoFlow { get; set; }

    public string CoverSpeech { get; set; } = string.Empty;

    public DateOnly CreatedOn { get; set; }

    public DateOnly UpdatedOn { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }
}
