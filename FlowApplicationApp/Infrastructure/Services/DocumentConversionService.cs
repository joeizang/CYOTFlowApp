using System.Text;
using System.Xml.Linq;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using FlowApplicationApp.Infrastructure.Services.Models;

namespace FlowApplicationApp.Infrastructure.Services;

public class DocumentConversionService : IDocumentConversionService
{
    private readonly ILogger<DocumentConversionService> _logger;

    public DocumentConversionService(ILogger<DocumentConversionService> logger)
    {
        _logger = logger;
    }

    public async Task<string> ConvertDocxToHtmlAsync(Stream docxStream, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await ConvertDocxToHtmlWithMetadataAsync(docxStream, cancellationToken);
            return result.Success ? result.HtmlContent : string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting DOCX to HTML");
            throw;
        }
    }

    public async Task<ConversionResult> ConvertDocxToHtmlWithMetadataAsync(Stream docxStream, CancellationToken cancellationToken = default)
    {
        var result = new ConversionResult
        {
            ConversionTimestamp = DateTime.UtcNow,
            FileSizeBytes = docxStream.Length
        };

        try
        {
            // Ensure stream is at the beginning
            if (docxStream.CanSeek)
            {
                docxStream.Position = 0;
            }

            using var wordDocument = WordprocessingDocument.Open(docxStream, false);
            
            if (wordDocument.MainDocumentPart == null)
            {
                result.Success = false;
                result.ErrorMessage = "Invalid document: Missing main document part";
                return result;
            }

            var body = wordDocument.MainDocumentPart.Document.Body;
            if (body == null)
            {
                result.Success = false;
                result.ErrorMessage = "Invalid document: Missing document body";
                return result;
            }

            var htmlBuilder = new StringBuilder();
            htmlBuilder.AppendLine("<div class=\"code-of-conduct-content\">");

            var wordCount = 0;

            // Process each element in the document body
            foreach (var element in body.Elements())
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (element is Paragraph paragraph)
                {
                    var (html, words) = ProcessParagraph(paragraph);
                    htmlBuilder.AppendLine(html);
                    wordCount += words;
                }
                else if (element is Table table)
                {
                    var (html, words) = ProcessTable(table);
                    htmlBuilder.AppendLine(html);
                    wordCount += words;
                }
            }

            htmlBuilder.AppendLine("</div>");

            result.HtmlContent = htmlBuilder.ToString();
            result.WordCount = wordCount;
            result.Success = true;

            // Extract metadata
            var docProperties = wordDocument.PackageProperties;
            result.Metadata["Title"] = docProperties.Title ?? string.Empty;
            result.Metadata["Author"] = docProperties.Creator ?? string.Empty;
            result.Metadata["LastModified"] = docProperties.Modified?.ToString() ?? string.Empty;

            _logger.LogInformation("Successfully converted DOCX to HTML. Word count: {WordCount}", wordCount);

            return await Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during DOCX to HTML conversion");
            result.Success = false;
            result.ErrorMessage = $"Conversion failed: {ex.Message}";
            return result;
        }
    }

    private (string Html, int WordCount) ProcessParagraph(Paragraph paragraph)
    {
        var htmlBuilder = new StringBuilder();
        var wordCount = 0;
        var textContent = new StringBuilder();
        var styles = new List<string>();

        // Check paragraph properties for styling
        var paragraphProperties = paragraph.ParagraphProperties;
        var isHeading = false;
        var headingLevel = 2; // Default heading level

        if (paragraphProperties != null)
        {
            var paragraphStyleId = paragraphProperties.ParagraphStyleId;
            if (paragraphStyleId != null)
            {
                var styleId = paragraphStyleId.Val?.Value ?? string.Empty;
                if (styleId.StartsWith("Heading"))
                {
                    isHeading = true;
                    headingLevel = int.TryParse(styleId.Replace("Heading", ""), out var level) ? level : 2;
                }
            }

            // Check for list items
            var numberingProperties = paragraphProperties.NumberingProperties;
            if (numberingProperties != null)
            {
                // This is a list item - we'll handle it as a simple bullet for now
                textContent.Append("• ");
            }
        }

        // Process runs (text segments with formatting)
        foreach (var run in paragraph.Elements<Run>())
        {
            var runProperties = run.RunProperties;
            var runStyles = new List<string>();

            if (runProperties != null)
            {
                if (runProperties.Bold != null) runStyles.Add("font-weight: bold;");
                if (runProperties.Italic != null) runStyles.Add("font-style: italic;");
                if (runProperties.Underline != null) runStyles.Add("text-decoration: underline;");
            }

            foreach (var text in run.Elements<Text>())
            {
                var content = text.Text;
                wordCount += content.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;

                if (runStyles.Any())
                {
                    textContent.Append($"<span style=\"{string.Join(" ", runStyles)}\">{System.Net.WebUtility.HtmlEncode(content)}</span>");
                }
                else
                {
                    textContent.Append(System.Net.WebUtility.HtmlEncode(content));
                }
            }
        }

        var finalText = textContent.ToString();
        
        if (string.IsNullOrWhiteSpace(finalText))
        {
            return ("<br/>", 0);
        }

        if (isHeading)
        {
            htmlBuilder.Append($"<h{headingLevel}>{finalText}</h{headingLevel}>");
        }
        else
        {
            htmlBuilder.Append($"<p>{finalText}</p>");
        }

        return (htmlBuilder.ToString(), wordCount);
    }

    private (string Html, int WordCount) ProcessTable(Table table)
    {
        var htmlBuilder = new StringBuilder();
        var wordCount = 0;

        htmlBuilder.AppendLine("<table class=\"table table-bordered\">");

        foreach (var row in table.Elements<TableRow>())
        {
            htmlBuilder.AppendLine("<tr>");

            foreach (var cell in row.Elements<TableCell>())
            {
                htmlBuilder.Append("<td>");

                foreach (var paragraph in cell.Elements<Paragraph>())
                {
                    var (html, words) = ProcessParagraph(paragraph);
                    htmlBuilder.Append(html);
                    wordCount += words;
                }

                htmlBuilder.AppendLine("</td>");
            }

            htmlBuilder.AppendLine("</tr>");
        }

        htmlBuilder.AppendLine("</table>");

        return (htmlBuilder.ToString(), wordCount);
    }
}
