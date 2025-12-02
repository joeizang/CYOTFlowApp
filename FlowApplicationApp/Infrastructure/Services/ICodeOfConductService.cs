using FlowApplicationApp.Data.DomainModels;

namespace FlowApplicationApp.Infrastructure.Services;

public interface ICodeOfConductService
{
    /// <summary>
    /// Uploads a new Code of Conduct document, converts it to HTML, and stores both versions
    /// </summary>
    /// <param name="file">The uploaded DOCX file</param>
    /// <param name="uploadedById">ID of the admin user uploading the document</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created CodeOfConductDocument entity</returns>
    Task<CodeOfConductDocument> UploadCodeOfConductAsync(IFormFile file, Guid uploadedById, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the currently active Code of Conduct document
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The active document or null if none exists</returns>
    Task<CodeOfConductDocument?> GetActiveCodeOfConductAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all versions of the Code of Conduct documents
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of all Code of Conduct documents ordered by version descending</returns>
    Task<List<CodeOfConductDocument>> GetAllVersionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a file stream for downloading the original DOCX document
    /// </summary>
    /// <param name="documentId">ID of the document to download</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>FileStream of the original DOCX file</returns>
    Task<FileStream> GetOriginalDocumentAsync(Guid documentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a specific version as the active Code of Conduct
    /// </summary>
    /// <param name="documentId">ID of the document to activate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> SetActiveVersionAsync(Guid documentId, CancellationToken cancellationToken = default);
}
