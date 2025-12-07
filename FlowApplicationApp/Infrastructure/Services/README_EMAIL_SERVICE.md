# Email Service Documentation

## Overview

The email service provides functionality to send emails to auditioners when they pass their audition, congratulating them and asking them to download and sign the Code of Conduct.

## Implementation Details

### Components

1. **IEmailSender Interface** (`Infrastructure/Services/IEmailSender.cs`)
   - Defines the contract for email sending operations
   - Methods:
     - `SendEmailAsync`: Generic email sending
     - `SendAuditionPassedEmailAsync`: Specialized method for audition passed notifications

2. **EmailSender Service** (`Infrastructure/Services/EmailSender.cs`)
   - Concrete implementation using MailKit
   - Uses Gmail SMTP (smtp.gmail.com:587)
   - Supports HTML email formatting

3. **Unit Tests** (`FlowApplicationApp.Tests/Infrastructure/Services/EmailSenderTests.cs`)
   - Verifies service initialization and method signatures
   - Includes skipped integration test for manual testing

## Configuration

The email service reads configuration from the `.env` file:

```env
# Email Settings
EMAIL_SENDER_EMAIL="sampleemail@gmail.com"
EMAIL_SENDER_PASSWORD="xxxxxxx"
EMAIL_SENDER_NAME="Courtyard of Truth Ministries"
```

The application loads these values using the `dotenv.net` package at startup.

**Important**: Never commit the `.env` file to source control. Add it to `.gitignore`.

## Usage Example

```csharp
// In your controller or service
public class AuditionsController : Controller
{
    private readonly IEmailSender _emailSender;
    
    public AuditionsController(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }
    
    public async Task<IActionResult> ApproveAudition(Guid auditionerId)
    {
        var auditioner = await _auditionerService.GetByIdAsync(auditionerId);
        
        // Send congratulations email
        var emailSent = await _emailSender.SendAuditionPassedEmailAsync(
            auditioner.Email,
            auditioner.FullName
        );
        
        if (emailSent)
        {
            // Process the auditioner to become a flow member
            // ...
        }
        
        return RedirectToAction("Index");
    }
}
```

## Email Template

The audition passed email includes:

- Congratulations message
- Instructions to download and sign the Code of Conduct
- Professional HTML formatting with styling
- Call-to-action button (placeholder link)

## Gmail Configuration

### App Password Setup

Since Gmail requires App Passwords for third-party applications:

1. Go to your Google Account settings
2. Enable 2-Factor Authentication
3. Go to Security > App Passwords
4. Generate an app password for "Mail"
5. Use this app password instead of your regular password

**Note**: The current password in the code may need to be updated with an App Password for Gmail to work properly.

## Testing

The tests use **NSubstitute** for mocking dependencies.

Run unit tests:

```bash

cd FlowApplicationApp.Tests
dotnet test
```

To test actual email sending, update the integration test with your email and remove the `Skip` attribute:

```csharp
[Fact] // Remove Skip parameter
public async Task SendEmail_Integration_ShouldSendRealEmail()
{
    // Replace "your-test-email@example.com" with your email
    var result = await emailSender.SendAuditionPassedEmailAsync(
        "your-email@example.com",
        "Test Auditioner"
    );
    
    Assert.True(result);
}
```

## Security Considerations

1. **Never commit the `.env` file** to source control - add it to `.gitignore`
2. All credentials are stored in the `.env` file, not in `appsettings.json`
3. For production, use environment variables or Azure Key Vault
4. Consider using SendGrid, AWS SES, or Azure Communication Services for production
5. Implement rate limiting to prevent email spam
6. Add retry logic with exponential backoff for failed sends

## Future Enhancements

- Add email templates using Razor or a templating engine
- Implement email queuing for bulk sends
- Add email tracking and delivery confirmation
- Support attachments (e.g., Code of Conduct PDF)
- Add CC and BCC support
- Implement email logging and audit trail
