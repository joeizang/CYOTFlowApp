namespace FlowApplicationApp.ViewModels.FlowMembers;

public class UploadMemberCodeOfConductViewModel
{
    public IFormFile? PdfFile { get; set; }
    
    public bool IsSuccess { get; set; }
    
    public string Message { get; set; } = string.Empty;
    
    public string? DownloadUrl { get; set; }
    
    public DateTime? UploadedAt { get; set; }
}
