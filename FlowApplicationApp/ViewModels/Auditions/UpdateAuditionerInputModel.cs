using System;
using System.ComponentModel.DataAnnotations;

namespace FlowApplicationApp.ViewModels.Auditions;

public class UpdateAuditionerInputModel
{
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Phone]
    [Display(Name = "WhatsApp Number")]
    public string WhatsAppNumber { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Date of Birth")]
    public DateOnly DoB { get; set; }

    [Required]
    [MaxLength(1000)]
    [Display(Name = "Biography")]
    public string Bio { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "When did you become born again?")]
    public DateOnly BornAgainDate { get; set; }

    [Display(Name = "Update Profile Image (Optional)")]
    public IFormFile? ProfileImage { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "When were you water baptized?")]
    public DateOnly WaterBaptismDate { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "When were you baptized in the Holy Spirit?")]
    public DateOnly HolySpiritBaptismDate { get; set; }

    [Required]
    [Display(Name = "Do you hear God speaking to you?")]
    public bool HearsGod { get; set; }

    [MaxLength(1000)]
    [Display(Name = "How did you start hearing God?")]
    public string HowTheyStartedHearingGod { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    [Display(Name = "Why do you want to join Flow?")]
    public string CoverSpeech { get; set; } = string.Empty;

    [Display(Name = "Shortlisted for Audition")]
    public bool ShortlistedForAudition { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Audition Date")]
    public DateOnly? AuditionDate { get; set; }

    [DataType(DataType.Time)]
    [Display(Name = "Audition Time")]
    public TimeOnly? AuditionTime { get; set; }

    [Display(Name = "Accepted into Flow")]
    public bool AcceptedIntoFlow { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; }
}
