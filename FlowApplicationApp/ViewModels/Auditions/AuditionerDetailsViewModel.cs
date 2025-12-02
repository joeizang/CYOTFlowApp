using System;

namespace FlowApplicationApp.ViewModels.Auditions;

public class AuditionerDetailsViewModel
{
    public Guid Id { get; set; }
    
    public string FullName { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;
    
    public DateTime DoB { get; set; }
    
    public string WhatsAppNumber { get; set; } = string.Empty;
    
    public string Bio { get; set; } = string.Empty;
    
    public DateTime BornAgainDate { get; set; }
    
    public string ProfileImageUrl { get; set; } = string.Empty;
    
    public DateTime WaterBaptismDate { get; set; }
    
    public DateTime HolySpiritBaptismDate { get; set; }
    
    public bool HearsGod { get; set; }
    
    public string HowTheyStartedHearingGod { get; set; } = string.Empty;
    
    public bool ShortlistedForAudition { get; set; }
    
    public DateTime AuditionDate { get; set; }
    
    public TimeOnly AuditionTime { get; set; }
    
    public bool AcceptedIntoFlow { get; set; }
    
    public string CoverSpeech { get; set; } = string.Empty;
    
    public bool IsActive { get; set; }
}
