using FlowApplicationApp.Data;
using FlowApplicationApp.Data.DomainModels;
using Microsoft.EntityFrameworkCore;

namespace FlowApplicationApp.Infrastructure.Services;

public class CodeOfConductService : ICodeOfConductService
{
    private readonly ApplicationDbContext _context;
    private readonly IDocumentConversionService _conversionService;
    private readonly ILogger<CodeOfConductService> _logger;
    private readonly IWebHostEnvironment _environment;
    private const string StorageFolder = "uploadFiles/code-of-conduct";
    private const long MaxFileSizeInBytes = 5 * 1024 * 1024; // 5MB

    public CodeOfConductService(
        ApplicationDbContext context,
        IDocumentConversionService conversionService,
        ILogger<CodeOfConductService> logger,
        IWebHostEnvironment environment)
    {
        _context = context;
        _conversionService = conversionService;
        _logger = logger;
        _environment = environment;
    }

    public async Task<CodeOfConductDocument> UploadCodeOfConductAsync(
        IFormFile file, 
        Guid uploadedById, 
        CancellationToken cancellationToken = default)
    {
        // Validate file
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is required", nameof(file));
        }

        if (file.Length > MaxFileSizeInBytes)
        {
            throw new InvalidOperationException($"File size exceeds maximum allowed size of {MaxFileSizeInBytes / (1024 * 1024)}MB");
        }

        if (!file.FileName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Only .docx files are allowed");
        }

        try
        {
            // Get the next version number
            var currentVersion = await _context.CodeOfConductDocuments
                .MaxAsync(d => (int?)d.Version, cancellationToken) ?? 0;
            var newVersion = currentVersion + 1;

            // Create storage directory
            var storagePath = Path.Combine(_environment.ContentRootPath, "..", "..", StorageFolder, $"v{newVersion}");
            Directory.CreateDirectory(storagePath);

            // Save the original file
            var fileName = $"code-of-conduct-v{newVersion}.docx";
            var filePath = Path.Combine(storagePath, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream, cancellationToken);
            }

            // Convert to HTML
            string htmlContent;
            using (var stream = file.OpenReadStream())
            {
                htmlContent = await _conversionService.ConvertDocxToHtmlAsync(stream, cancellationToken);
            }

            if (string.IsNullOrWhiteSpace(htmlContent))
            {
                // Clean up the file if conversion failed
                File.Delete(filePath);
                throw new InvalidOperationException("Failed to convert document to HTML");
            }

            // Deactivate all existing documents
            var existingDocuments = await _context.CodeOfConductDocuments
                .Where(d => d.IsActive)
                .ToListAsync(cancellationToken);

            foreach (var doc in existingDocuments)
            {
                doc.IsActive = false;
            }

            // Create new document record
            var document = new CodeOfConductDocument
            {
                Id = Guid.NewGuid(),
                FileName = fileName,
                OriginalFilePath = filePath,
                HtmlContent = htmlContent,
                UploadedBy = uploadedById,
                UploadedAt = DateTime.UtcNow,
                Version = newVersion,
                IsActive = true,
                FileSize = file.Length
            };

            _context.CodeOfConductDocuments.Add(document);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Code of Conduct v{Version} uploaded successfully by user {UserId}", 
                newVersion, 
                uploadedById);

            return document;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading Code of Conduct document");
            throw;
        }
    }

    public async Task<CodeOfConductDocument?> GetActiveCodeOfConductAsync(CancellationToken cancellationToken = default)
    {
        return await _context.CodeOfConductDocuments
            .Include(d => d.UploadedByMember)
            .FirstOrDefaultAsync(d => d.IsActive, cancellationToken);
    }

    public async Task<List<CodeOfConductDocument>> GetAllVersionsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.CodeOfConductDocuments
            .Include(d => d.UploadedByMember)
            .OrderByDescending(d => d.Version)
            .ToListAsync(cancellationToken);
    }

    public async Task<FileStream> GetOriginalDocumentAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var document = await _context.CodeOfConductDocuments
            .FirstOrDefaultAsync(d => d.Id == documentId, cancellationToken);

        if (document == null)
        {
            throw new FileNotFoundException("Document not found");
        }

        if (!File.Exists(document.OriginalFilePath))
        {
            _logger.LogError("File not found at path: {Path}", document.OriginalFilePath);
            throw new FileNotFoundException("Document file not found on disk", document.OriginalFilePath);
        }

        return new FileStream(document.OriginalFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
    }

    public async Task<bool> SetActiveVersionAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var document = await _context.CodeOfConductDocuments
            .FirstOrDefaultAsync(d => d.Id == documentId, cancellationToken);

        if (document == null)
        {
            return false;
        }

        // Deactivate all documents
        var allDocuments = await _context.CodeOfConductDocuments.ToListAsync(cancellationToken);
        foreach (var doc in allDocuments)
        {
            doc.IsActive = false;
        }

        // Activate the selected document
        document.IsActive = true;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Code of Conduct v{Version} set as active", document.Version);

        return true;
    }
}
