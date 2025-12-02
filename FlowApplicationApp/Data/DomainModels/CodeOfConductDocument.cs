namespace FlowApplicationApp.Data.DomainModels;

public class CodeOfConductDocument
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFilePath { get; set; } = string.Empty;
    public string HtmlContent { get; set; } = string.Empty;
    public Guid UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; }
    public int Version { get; set; }
    public bool IsActive { get; set; }
    public long FileSize { get; set; }
    
    // Navigation property
    public FlowMember? UploadedByMember { get; set; }
}
