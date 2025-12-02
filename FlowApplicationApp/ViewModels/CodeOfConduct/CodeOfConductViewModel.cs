namespace FlowApplicationApp.ViewModels.CodeOfConduct;

public class CodeOfConductViewModel
{
    public string HtmlContent { get; set; } = string.Empty;
    public DateTime? UploadedAt { get; set; }
    public int Version { get; set; }
    public bool CanEdit { get; set; }
    public Guid? DocumentId { get; set; }
    public string? UploadedByName { get; set; }
}
