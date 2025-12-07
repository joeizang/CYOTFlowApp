# Code of Conduct Document Management System

## Overview
A comprehensive document management system that allows administrators to upload Word documents (.docx) for the Code of Conduct, automatically converts them to HTML for web display, and makes them available for download to all users (authenticated or not).

## Features

### ✅ Implemented Features

1. **Admin Upload** - Admins can upload .docx files
2. **Automatic Conversion** - DOCX files are automatically converted to HTML using DocumentFormat.OpenXml
3. **Public Access** - All users can view and download the Code of Conduct (no authentication required)
4. **Version Management** - All uploaded versions are tracked and stored
5. **Active Version Control** - Only one version is active at a time
6. **Download Support** - Original .docx files can be downloaded
7. **Audit Trail** - Tracks who uploaded each version and when
8. **File Validation** - Validates file type and size (max 5MB)

## File Structure

```
Infrastructure/
└── Services/
    ├── IDocumentConversionService.cs      # Generic DOCX to HTML conversion interface
    ├── DocumentConversionService.cs       # Implementation using DocumentFormat.OpenXml
    ├── ICodeOfConductService.cs           # Business logic interface
    ├── CodeOfConductService.cs            # Business logic implementation
    └── Models/
        └── ConversionResult.cs            # Conversion result model

Data/
├── DomainModels/
│   └── CodeOfConductDocument.cs          # Entity model
└── ModelConfigurations/
    └── CodeOfConductDocumentEntityTypeConfiguration.cs  # EF configuration

ViewModels/
└── CodeOfConduct/
    ├── CodeOfConductViewModel.cs         # Display view model
    ├── UploadCodeOfConductViewModel.cs   # Upload view model
    └── ManageVersionsViewModel.cs        # Version management view model

Controllers/
└── CodeOfConductController.cs            # Controller with all endpoints

Views/
└── CodeOfConduct/
    ├── Index.cshtml                      # Public view (displays HTML)
    ├── Upload.cshtml                     # Admin upload form
    └── Versions.cshtml                   # Admin version management
```

## API Endpoints

### Public Endpoints (No Authentication Required)

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/code-of-conduct` | View active Code of Conduct |
| GET | `/code-of-conduct/download` | Download active .docx file |
| GET | `/code-of-conduct/download/{id}` | Download specific version |

### Admin Endpoints (Requires Admin Role)

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/code-of-conduct/upload` | Show upload form |
| POST | `/code-of-conduct/upload` | Handle file upload |
| GET | `/code-of-conduct/versions` | List all versions |
| POST | `/code-of-conduct/activate/{id}` | Activate specific version |

## Database Schema

### CodeOfConductDocuments Table

| Column | Type | Description |
|--------|------|-------------|
| Id | Guid | Primary key |
| FileName | string | Original file name |
| OriginalFilePath | string | Path to stored .docx |
| HtmlContent | string | Converted HTML content |
| UploadedBy | Guid | Foreign key to FlowMember |
| UploadedAt | DateTime | Upload timestamp |
| Version | int | Version number (auto-incremented) |
| IsActive | bool | Whether this is the active version |
| FileSize | long | File size in bytes |

### Indexes

- `IX_CodeOfConductDocuments_IsActive` - For quick retrieval of active document
- `IX_CodeOfConductDocuments_Version` - For version sorting
- `IX_CodeOfConductDocuments_UploadedBy` - Foreign key index

## Usage

### For Administrators

1. **Upload New Code of Conduct**
   - Navigate to `/code-of-conduct/upload`
   - Select a .docx file (max 5MB)
   - Click "Upload Document"
   - Document is automatically converted and set as active

2. **Manage Versions**
   - Navigate to `/code-of-conduct/versions`
   - View all uploaded versions
   - Download any version
   - Activate a different version if needed

### For All Users

1. **View Code of Conduct**
   - Navigate to `/code-of-conduct`
   - View the HTML-rendered document

2. **Download Original Document**
   - Click "Download DOCX" button on the Code of Conduct page
   - Original .docx file will be downloaded

## Technical Details

### Dependencies

- **DocumentFormat.OpenXml** (v3.3.0) - For DOCX parsing and conversion
- **Entity Framework Core** - For database operations
- **ASP.NET Core Identity** - For authentication and authorization

### Services Registration (Program.cs)

```csharp
builder.Services.AddScoped<IDocumentConversionService, DocumentConversionService>();
builder.Services.AddScoped<ICodeOfConductService, CodeOfConductService>();
```

### File Storage

- Files are stored in: `uploadFiles/code-of-conduct/v{version}/`
- Naming convention: `code-of-conduct-v{version}.docx`
- HTML content is stored in the database for fast retrieval

### Conversion Features

The DOCX to HTML converter supports:

- ✅ Paragraphs and line breaks
- ✅ Headings (H1-H6)
- ✅ Text formatting (bold, italic, underline)
- ✅ Tables with borders
- ✅ Lists (rendered with bullet points)
- ✅ Basic styling preservation

## Security Considerations

1. **Authorization**
   - Upload functionality restricted to Admin role
   - Public endpoints allow anonymous access
   - Anti-forgery tokens on all POST actions

2. **Validation**
   - File type validation (only .docx allowed)
   - File size validation (max 5MB)
   - Path validation to prevent directory traversal

3. **Error Handling**
   - Graceful error handling with user-friendly messages
   - Comprehensive logging for debugging
   - Database transaction rollback on failures

## Future Enhancements

Possible improvements:

- [ ] Image extraction and embedding
- [ ] Custom CSS styling for converted HTML
- [ ] PDF export functionality
- [ ] Document comparison between versions
- [ ] Search within document content
- [ ] Comments/feedback system
- [ ] Email notifications on new uploads
- [ ] Document approval workflow

## Testing

To test the implementation:

1. **Build and Run**

   ```bash
   cd FlowApplicationApp
   dotnet build
   dotnet run
   ```

2. **Login as Admin**
   - Use the seeded admin account

3. **Upload a Test Document**
   - Navigate to `/code-of-conduct/upload`
   - Upload a .docx file
   - Verify conversion and display

4. **Test Public Access**
   - Log out
   - Navigate to `/code-of-conduct`
   - Verify content is visible
   - Test download functionality

## Migration

The database migration has been created and applied:

- Migration: `20251201192345_AddCodeOfConductDocument`
- Table: `CodeOfConductDocuments`

To recreate the migration:

```bash
dotnet ef migrations add AddCodeOfConductDocument
dotnet ef database update
```

## Support

For issues or questions:

- Check logs in the application
- Review error messages in the UI
- Verify file permissions for the upload directory
- Ensure the database migration has been applied
