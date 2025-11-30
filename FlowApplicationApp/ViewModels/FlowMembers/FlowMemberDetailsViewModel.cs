using System;

namespace FlowApplicationApp.ViewModels.FlowMembers;

public class FlowMemberDetailsViewModel
{
    public Guid Id { get; set; }
    
    public string FullName { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;
    
    public string UserName { get; set; } = string.Empty;
    
    public DateOnly DoB { get; set; }
    
    public string WhatsAppNumber { get; set; } = string.Empty;
    
    public string Bio { get; set; } = string.Empty;
    
    public DateOnly BornAgainDate { get; set; }
    
    public string ProfileImageUrl { get; set; } = string.Empty;
    
    public DateOnly WaterBaptismDate { get; set; }
    
    public DateOnly HolySpiritBaptismDate { get; set; }
    
    public bool HearsGod { get; set; }
    
    public string HowTheyStartedHearingGod { get; set; } = string.Empty;
    
    public string CoverSpeech { get; set; } = string.Empty;
    
    public DateOnly CreatedOn { get; set; }
    
    public DateOnly UpdatedOn { get; set; }
    
    public bool IsActive { get; set; }
    
    public List<string> Roles { get; set; } = new();
}
