# Quick Start: Email Service

## What It Does

Automatically sends a congratulations email to auditioners when they pass their audition and are accepted into Flow.

## Files Added

- `Infrastructure/Services/IEmailSender.cs` - Interface
- `Infrastructure/Services/EmailSender.cs` - Implementation
- `FlowApplicationApp.Tests/Infrastructure/Services/EmailSenderTests.cs` - Unit tests

## Configuration (.env file)

```env
# Email Settings
EMAIL_SENDER_EMAIL="courtyardoftruth@gmail.com"
EMAIL_SENDER_PASSWORD="tRuthIsL!ght"
EMAIL_SENDER_NAME="Courtyard of Truth"
```

Add these settings to the `.env` file in the FlowApplicationApp directory.

## How It Works

When an admin updates an auditioner's status to `AcceptedIntoFlow = true` in the `AuditionsController.Update()` method, the system:

1. Sends a congratulations email
2. Creates a FlowMember record
3. Logs the email status

## Email Content

- Professional HTML template
- Congratulations message
- Instructions to download and sign Code of Conduct
- Styled with CSS for professional appearance

## Testing

Tests use **NSubstitute** for mocking (not Moq).

```bash
cd FlowApplicationApp.Tests
dotnet test
```

**Result**: 8 tests passing ✅

## Important: Gmail App Password

⚠️ Gmail requires an App Password for third-party apps:

1. Enable 2FA on Gmail account
2. Generate App Password: Google Account → Security → App Passwords
3. Replace password in appsettings.json

## Usage in Code

```csharp
// Injected via DI in AuditionsController
private readonly IEmailSender _emailSender;

// Send email
var result = await _emailSender.SendAuditionPassedEmailAsync(
    "auditioner@example.com",
    "John Doe",
    cancellationToken
);
```

## Security Note

⚠️ **Critical**: Never commit the `.env` file to source control!

For production, use:

- Environment variables on the server
- Azure Key Vault
- Azure App Service Application Settings

The `.env` file should be added to `.gitignore`.

## Full Documentation

See `Infrastructure/Services/README_EMAIL_SERVICE.md` and `EMAIL_IMPLEMENTATION_SUMMARY.md`
