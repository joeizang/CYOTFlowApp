using FlowApplicationApp.Infrastructure.Services.Models;

namespace FlowApplicationApp.Infrastructure.Services;

public interface IDocumentConversionService
{
    /// <summary>
    /// Converts a DOCX document stream to HTML
    /// </summary>
    /// <param name="docxStream">The input DOCX file stream</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>HTML string representation of the document</returns>
    Task<string> ConvertDocxToHtmlAsync(Stream docxStream, CancellationToken cancellationToken = default);

    /// <summary>
    /// Converts a DOCX document stream to HTML with additional metadata
    /// </summary>
    /// <param name="docxStream">The input DOCX file stream</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>ConversionResult containing HTML and metadata</returns>
    Task<ConversionResult> ConvertDocxToHtmlWithMetadataAsync(Stream docxStream, CancellationToken cancellationToken = default);
}
