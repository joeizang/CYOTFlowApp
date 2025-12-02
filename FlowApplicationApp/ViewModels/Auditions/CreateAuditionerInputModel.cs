using System;
using System.ComponentModel.DataAnnotations;

namespace FlowApplicationApp.ViewModels.Auditions;

public class CreateAuditionerInputModel
{
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
    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Date of Birth")]
    public DateTime DoB { get; set; } = new DateTime(1990, 1, 1);

    [Required]
    [MaxLength(1000)]
    [Display(Name = "Biography")]
    public string Bio { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "When did you become born again?")]
    public DateTime BornAgainDate { get; set; } = DateTime.Now;

    [Required]
    [Display(Name = "Add a Profile Image")]
    public IFormFile ProfileImage { get; set; } = null!;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "When were you water baptized?")]
    public DateTime WaterBaptismDate { get; set; } = DateTime.Now;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "When were you baptized in the Holy Spirit?")]
    public DateTime HolySpiritBaptismDate { get; set; } = DateTime.Now;

    [Required]
    [Display(Name = "Do you hear God speaking to you?")]
    public bool HearsGod { get; set; }

    [Required]
    [MaxLength(1000)]
    [Display(Name = "How did you start hearing God?")]
    public string HowTheyStartedHearingGod { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    [Display(Name = "Why do you want to join Flow?")]
    public string CoverSpeech { get; set; } = string.Empty;

    [Required]
    [Display(Name = "I have read and agree to the Code of Conduct")]
    public bool HaveReadCoC { get; set; }
}
