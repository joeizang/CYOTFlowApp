namespace FlowApplicationApp.Infrastructure.Services.Models;

public class ConversionResult
{
    public string HtmlContent { get; set; } = string.Empty;
    public int WordCount { get; set; }
    public int PageCount { get; set; }
    public long FileSizeBytes { get; set; }
    public DateTime ConversionTimestamp { get; set; }
    public List<string> ExtractedImages { get; set; } = new();
    public Dictionary<string, string> Metadata { get; set; } = new();
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}
