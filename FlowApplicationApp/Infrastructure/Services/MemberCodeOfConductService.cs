using FlowApplicationApp.Data;
using FlowApplicationApp.Data.DomainModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FlowApplicationApp.Infrastructure.Services;

public class MemberCodeOfConductService : IMemberCodeOfConductService
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<MemberCodeOfConductService> _logger;
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB
    private static readonly string[] AllowedExtensions = { ".pdf" };

    public MemberCodeOfConductService(
        ApplicationDbContext context,
        IWebHostEnvironment environment,
        ILogger<MemberCodeOfConductService> logger)
    {
        _context = context;
        _environment = environment;
        _logger = logger;
    }

    public async Task<string> UploadMemberCodeOfConductAsync(IFormFile file, Guid memberId, CancellationToken cancellationToken)
    {
        // Validate file
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is required", nameof(file));
        }

        if (file.Length > MaxFileSize)
        {
            throw new InvalidOperationException($"File size exceeds maximum allowed size of {MaxFileSize / 1024 / 1024}MB");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Only PDF files are allowed");
        }

        // Get member
        var member = await _context.FlowMembers.FindAsync(new object[] { memberId }, cancellationToken);
        if (member == null)
        {
            throw new InvalidOperationException("Member not found");
        }

        try
        {
            // Delete existing file if present
            if (!string.IsNullOrEmpty(member.CodeOfConductPdfPath))
            {
                await DeleteMemberCodeOfConductAsync(memberId, cancellationToken);
            }

            // Create directory structure: uploadFiles/code-of-conduct/members/{memberId}/
            var uploadPath = Path.GetFullPath(
                Path.Combine(_environment.ContentRootPath, "../uploadFiles/code-of-conduct/members", memberId.ToString()) // server is different ../../ goes too far
            );

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // Generate file name
            var fileName = $"CodeOfConduct_{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
            var filePath = Path.Combine(uploadPath, fileName);

            // Save file
            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream, cancellationToken).ConfigureAwait(false);
            }

            // Update database
            var relativePath = Path.Combine("code-of-conduct/members", memberId.ToString(), fileName);
            member.CodeOfConductPdfPath = relativePath;
            member.HasUploadedCodeOfConduct = true;
            member.CodeOfConductUploadedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Member {MemberId} uploaded Code of Conduct PDF: {FileName}", memberId, fileName);

            return relativePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading Code of Conduct for member {MemberId}", memberId);
            throw;
        }
    }

    public async Task<bool> HasUploadedCodeOfConductAsync(Guid memberId, CancellationToken cancellationToken) =>
            await _context.FlowMembers.AsNoTracking()
            .Where(m => m.Id == memberId)
            .Select(m => m.HasUploadedCodeOfConduct)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<FileStream?> GetMemberCodeOfConductAsync(Guid memberId, CancellationToken cancellationToken)
    {
        var member = await _context.FlowMembers.AsNoTracking()
            .Where(m => m.Id == memberId)
            .Select(m => new { m.CodeOfConductPdfPath })
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        if (member == null || string.IsNullOrEmpty(member.CodeOfConductPdfPath))
        {
            return null;
        }

        var filePath = Path.GetFullPath(
            Path.Combine(_environment.ContentRootPath, "../../uploadFiles", member.CodeOfConductPdfPath)
        );

        if (!File.Exists(filePath))
        {
            _logger.LogWarning("Code of Conduct PDF not found at path: {FilePath}", filePath);
            return null;
        }

        return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
    }

    public async Task<bool> DeleteMemberCodeOfConductAsync(Guid memberId, CancellationToken cancellationToken = default)
    {
        var member = await _context.FlowMembers.FindAsync(new object[] { memberId }, cancellationToken);
        
        if (member == null || string.IsNullOrEmpty(member.CodeOfConductPdfPath))
        {
            return false;
        }

        try
        {
            var filePath = Path.GetFullPath(
                Path.Combine(_environment.ContentRootPath, "../../uploadFiles", member.CodeOfConductPdfPath)
            );

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            // Update database
            member.CodeOfConductPdfPath = null;
            member.HasUploadedCodeOfConduct = false;
            member.CodeOfConductUploadedAt = null;

            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Deleted Code of Conduct PDF for member {MemberId}", memberId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting Code of Conduct for member {MemberId}", memberId);
            return false;
        }
    }

    public async Task<string?> GetMemberCodeOfConductFileNameAsync(Guid memberId, CancellationToken cancellationToken)
    {
        var member = await _context.FlowMembers.AsNoTracking()
            .Where(m => m.Id == memberId)
            .Select(m => m.CodeOfConductPdfPath)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (string.IsNullOrEmpty(member))
        {
            return null;
        }

        return Path.GetFileName(member);
    }
}
