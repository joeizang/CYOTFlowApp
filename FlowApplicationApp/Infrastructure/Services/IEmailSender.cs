namespace FlowApplicationApp.Infrastructure.Services;

/// <summary>
/// Interface for sending emails
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Sends an email to a recipient
    /// </summary>
    /// <param name="toEmail">Recipient's email address</param>
    /// <param name="toName">Recipient's name</param>
    /// <param name="subject">Email subject</param>
    /// <param name="body">Email body (HTML supported)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if email sent successfully, false otherwise</returns>
    Task<bool> SendEmailAsync(string toEmail, string toName, string subject, string body, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Sends an email to an auditioner congratulating them on passing their audition
    /// </summary>
    /// <param name="auditionerEmail">Auditioner's email address</param>
    /// <param name="auditionerName">Auditioner's name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if email sent successfully, false otherwise</returns>
    Task<bool> SendAuditionPassedEmailAsync(string auditionerEmail, string auditionerName, CancellationToken cancellationToken = default);
}
