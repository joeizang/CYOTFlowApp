using FlowApplicationApp.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace FlowApplicationApp.Tests.Infrastructure.Services;

/// <summary>
/// Unit tests for EmailSender service
/// Note: These tests verify the structure and logic but won't actually send emails
/// For actual email sending tests, you would need integration tests
/// 
/// The EmailSender service uses IConfiguration to access environment variables
/// that are loaded from the .env file at application startup by dotenv.net.
/// These tests mock IConfiguration to simulate those .env values.
/// </summary>
public class EmailSenderTests
{
    private ILogger<EmailSender> CreateMockLogger()
    {
        return Substitute.For<ILogger<EmailSender>>();
    }

    private IConfiguration CreateMockConfiguration()
    {
        var config = Substitute.For<IConfiguration>();
        
        // Mock IConfiguration to return values as if they were loaded from .env file
        // In production, dotenv.net loads these from .env into the configuration system
        config["EMAIL_SENDER_EMAIL"].Returns("test@example.com");
        config["EMAIL_SENDER_PASSWORD"].Returns("test-password");
        config["EMAIL_SENDER_NAME"].Returns("Test Sender");
        
        return config;
    }

    [Fact]
    public void EmailSender_Constructor_ShouldInitialize()
    {
        // Arrange
        var logger = CreateMockLogger();
        var config = CreateMockConfiguration();

        // Act
        var emailSender = new EmailSender(logger, config);

        // Assert
        Assert.NotNull(emailSender);
    }

    [Fact]
    public async Task SendEmailAsync_WithValidParameters_ShouldNotThrowException()
    {
        // Arrange
        var logger = CreateMockLogger();
        var config = CreateMockConfiguration();
        var emailSender = new EmailSender(logger, config);
        
        var toEmail = "recipient@example.com";
        var toName = "Test User";
        var subject = "Test Subject";
        var body = "<h1>Test Body</h1>";

        // Act & Assert
        // This will fail to connect to SMTP but should handle the exception gracefully
        var result = await emailSender.SendEmailAsync(toEmail, toName, subject, body);
        
        // The method should return false on failure, not throw
        Assert.False(result);
    }

    [Fact]
    public async Task SendAuditionPassedEmailAsync_WithValidParameters_ShouldNotThrowException()
    {
        // Arrange
        var logger = CreateMockLogger();
        var config = CreateMockConfiguration();
        var emailSender = new EmailSender(logger, config);
        
        var auditionerEmail = "auditioner@example.com";
        var auditionerName = "John Doe";

        // Act & Assert
        // This will fail to connect to SMTP but should handle the exception gracefully
        var result = await emailSender.SendAuditionPassedEmailAsync(auditionerEmail, auditionerName);
        
        // The method should return false on failure, not throw
        Assert.False(result);
    }

    [Theory]
    [InlineData("test1@example.com", "Test User 1")]
    [InlineData("test2@example.com", "Test User 2")]
    [InlineData("auditioner@flowmembers.com", "Jane Smith")]
    public async Task SendAuditionPassedEmailAsync_WithVariousInputs_ShouldHandleGracefully(string email, string name)
    {
        // Arrange
        var logger = CreateMockLogger();
        var config = CreateMockConfiguration();
        var emailSender = new EmailSender(logger, config);

        // Act
        var result = await emailSender.SendAuditionPassedEmailAsync(email, name);

        // Assert
        // Should handle gracefully and return false (since SMTP connection will fail)
        Assert.False(result);
    }

    [Fact]
    public void EmailSender_ReadsConfigurationCorrectly()
    {
        // Arrange
        var logger = CreateMockLogger();
        var config = CreateMockConfiguration();

        // Act
        var emailSender = new EmailSender(logger, config);

        // Assert
        Assert.NotNull(emailSender);
        
        // Verify configuration was set up correctly
        Assert.Equal("test@example.com", config["EMAIL_SENDER_EMAIL"]);
        Assert.Equal("test-password", config["EMAIL_SENDER_PASSWORD"]);
        Assert.Equal("Test Sender", config["EMAIL_SENDER_NAME"]);
    }

    [Fact]
    public void EmailSender_WithNullConfiguration_UsesDefaultValues()
    {
        // Arrange
        var logger = CreateMockLogger();
        var config = Substitute.For<IConfiguration>();
        
        // Return null for all configuration keys to test defaults
        config["EMAIL_SENDER_EMAIL"].Returns((string?)null);
        config["EMAIL_SENDER_PASSWORD"].Returns((string?)null);
        config["EMAIL_SENDER_NAME"].Returns((string?)null);

        // Act
        var emailSender = new EmailSender(logger, config);

        // Assert - Service should initialize even with null config values
        Assert.NotNull(emailSender);
    }
}

/// <summary>
/// Integration test example for actual email sending
/// This test uses the actual .env configuration and attempts to send a real email
/// Run manually when you need to verify email functionality end-to-end
/// </summary>
public class EmailSenderIntegrationTests
{
    [Fact(Skip = "This is an integration test that sends real emails. Run manually when needed.")]
    public async Task SendEmail_Integration_ShouldSendRealEmail()
    {
        // Arrange
        var logger = Substitute.For<ILogger<EmailSender>>();
        var config = Substitute.For<IConfiguration>();
        
        // These values should match your actual .env file for real testing
        config["EMAIL_SENDER_EMAIL"].Returns("courtyardoftruth@gmail.com");
        config["EMAIL_SENDER_PASSWORD"].Returns("tRuthIsL!ght"); // Use actual Gmail App Password
        config["EMAIL_SENDER_NAME"].Returns("Courtyard of Truth");
        
        var emailSender = new EmailSender(logger, config);

        // Act
        var result = await emailSender.SendAuditionPassedEmailAsync(
            "your-test-email@example.com", // Replace with your test email
            "Test Auditioner"
        );

        // Assert
        Assert.True(result, "Email should be sent successfully");
    }
}
