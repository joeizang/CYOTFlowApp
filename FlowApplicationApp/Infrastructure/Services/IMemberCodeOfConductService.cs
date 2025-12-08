using FlowApplicationApp.Data.DomainModels;
using Microsoft.AspNetCore.Http;

namespace FlowApplicationApp.Infrastructure.Services;

public interface IMemberCodeOfConductService
{
    /// <summary>
    /// Uploads a signed Code of Conduct PDF for a member
    /// </summary>
    /// <param name="file">The uploaded PDF file</param>
    /// <param name="memberId">ID of the member uploading the document</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>File path where the PDF was saved</returns>
    Task<string> UploadMemberCodeOfConductAsync(IFormFile file, Guid memberId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a member has uploaded their Code of Conduct PDF
    /// </summary>
    /// <param name="memberId">ID of the member to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if member has uploaded their PDF, false otherwise</returns>
    Task<bool> HasUploadedCodeOfConductAsync(Guid memberId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a file stream for downloading a member's Code of Conduct PDF
    /// </summary>
    /// <param name="memberId">ID of the member whose document to retrieve</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>FileStream of the PDF file or null if not found</returns>
    Task<FileStream?> GetMemberCodeOfConductAsync(Guid memberId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a member's Code of Conduct PDF
    /// </summary>
    /// <param name="memberId">ID of the member whose document to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> DeleteMemberCodeOfConductAsync(Guid memberId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the file name of a member's Code of Conduct PDF
    /// </summary>
    /// <param name="memberId">ID of the member</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>File name or null if not found</returns>
    Task<string?> GetMemberCodeOfConductFileNameAsync(Guid memberId, CancellationToken cancellationToken = default);
}
