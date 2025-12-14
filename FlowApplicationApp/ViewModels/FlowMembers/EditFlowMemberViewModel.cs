using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace FlowApplicationApp.ViewModels.FlowMembers;

public class EditFlowMemberViewModel
{
    public Guid Id { get; set; }
    
    [Required(ErrorMessage = "First name is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "First name must be between 3 and 50 characters")]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Last name must be between 3 and 50 characters")]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please provide a valid email address")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Date of birth is required")]
    [DataType(DataType.Date)]
    [Display(Name = "Date of Birth")]
    public DateTime DoB { get; set; }
    
    [Required(ErrorMessage = "WhatsApp number is required")]
    [StringLength(40, MinimumLength = 11, ErrorMessage = "WhatsApp number must be between 11 and 40 characters")]
    [Display(Name = "WhatsApp Number")]
    public string WhatsAppNumber { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Biography is required")]
    [StringLength(1000, MinimumLength = 50, ErrorMessage = "Biography must be between 50 and 1000 characters")]
    [Display(Name = "Biography")]
    [DataType(DataType.MultilineText)]
    public string Bio { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Born again date is required")]
    [DataType(DataType.Date)]
    [Display(Name = "Born Again Date")]
    public DateTime BornAgainDate { get; set; }
    
    [StringLength(150, MinimumLength = 10, ErrorMessage = "Profile image URL must be between 10 and 150 characters")]
    [Display(Name = "Profile Image URL")]
    public string ProfileImageUrl { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Water baptism date is required")]
    [DataType(DataType.Date)]
    [Display(Name = "Water Baptism Date")]
    public DateTime WaterBaptismDate { get; set; }
    
    [Required(ErrorMessage = "Holy Spirit baptism date is required")]
    [DataType(DataType.Date)]
    [Display(Name = "Holy Spirit Baptism Date")]
    public DateTime HolySpiritBaptismDate { get; set; }
    
    [Display(Name = "Hears God")]
    public bool HearsGod { get; set; }
    
    [StringLength(500, MinimumLength = 100, ErrorMessage = "This field must be between 100 and 500 characters")]
    [Display(Name = "How They Started Hearing God")]
    [DataType(DataType.MultilineText)]
    public string HowTheyStartedHearingGod { get; set; } = string.Empty;
    
    [Display(Name = "Cover Speech / Why They Joined Flow")]
    [DataType(DataType.MultilineText)]
    public string CoverSpeech { get; set; } = string.Empty;
    
    [Display(Name = "Is Active")]
    public bool IsActive { get; set; }
    
    // Code of Conduct Properties
    [Display(Name = "Code of Conduct PDF")]
    public IFormFile? CodeOfConductPdf { get; set; }
    
    public bool HasUploadedCodeOfConduct { get; set; }
    
    public DateTime? CodeOfConductUploadedAt { get; set; }
    
    public string? CurrentCodeOfConductPdfPath { get; set; }
}
