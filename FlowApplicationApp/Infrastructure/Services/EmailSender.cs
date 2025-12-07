using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Logging;

namespace FlowApplicationApp.Infrastructure.Services;

/// <summary>
/// Email sender implementation using MailKit and Gmail SMTP
/// </summary>
public class EmailSender : IEmailSender
{
    private readonly ILogger<EmailSender> _logger;
    private readonly IConfiguration _configuration;
    private const string SmtpServer = "smtp.gmail.com";
    private const int SmtpPort = 587;

    public EmailSender(ILogger<EmailSender> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    /// <inheritdoc/>
    public async Task<bool> SendEmailAsync(string toEmail, string toName, string subject, string body, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = new MimeMessage();
            
            // Get credentials from environment variables (loaded from .env)
            var senderEmail = _configuration["EMAIL_SENDER_EMAIL"] ?? "courtyardoftruth@gmail.com";
            var senderPassword = _configuration["EMAIL_SENDER_PASSWORD"] ?? "tRuthIsL!ght";
            var senderName = _configuration["EMAIL_SENDER_NAME"] ?? "Courtyard of Truth";
            
            message.From.Add(new MailboxAddress(senderName, senderEmail));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = body
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            
            // Connect to Gmail's SMTP server
            await client.ConnectAsync(SmtpServer, SmtpPort, SecureSocketOptions.StartTls, cancellationToken);
            
            // Authenticate
            await client.AuthenticateAsync(senderEmail, senderPassword, cancellationToken);
            
            // Send the email
            await client.SendAsync(message, cancellationToken);
            
            // Disconnect
            await client.DisconnectAsync(true, cancellationToken);
            
            _logger.LogInformation("Email sent successfully to {Email}", toEmail);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> SendAuditionPassedEmailAsync(string auditionerEmail, string auditionerName, CancellationToken cancellationToken = default)
    {
        var subject = "Congratulations! You've Passed Your Audition";
        
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .button {{ display: inline-block; padding: 10px 20px; background-color: #4CAF50; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Congratulations, {auditionerName}!</h1>
        </div>
        <div class='content'>
            <h2>You've Successfully Passed Your Audition!</h2>
            <p>We are thrilled to inform you that you have passed your audition and are one step closer to becoming a Flow Member.</p>
            
            <p><strong>Next Steps:</strong></p>
            <ol>
                <li>Download and review our Code of Conduct document</li>
                <li>Sign the Code of Conduct</li>
                <li>Submit the signed document to complete your onboarding</li>
            </ol>
            
            <p>Please log in to your account to download the Code of Conduct:</p>
            <a href='#' class='button'>Download Code of Conduct</a>
            
            <p>Once you've reviewed and signed the document, you'll officially become a Flow Member!</p>
            
            <p>If you have any questions, please don't hesitate to reach out to us.</p>
            
            <p>Welcome to the family!</p>
        </div>
        <div class='footer'>
            <p>&copy; {DateTime.Now.Year} Courtyard of Truth. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

        return await SendEmailAsync(auditionerEmail, auditionerName, subject, body, cancellationToken);
    }
}
