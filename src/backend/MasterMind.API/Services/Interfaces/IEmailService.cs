namespace MasterMind.API.Services.Interfaces;

/// <summary>
/// Email service interface for sending emails
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Send an OTP email
    /// </summary>
    /// <param name="email">Recipient email address</param>
    /// <param name="otp">OTP code</param>
    /// <returns>True if email was sent successfully</returns>
    Task<bool> SendOtpEmailAsync(string email, string otp);

    /// <summary>
    /// Send a custom email
    /// </summary>
    /// <param name="to">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="body">Email body (HTML)</param>
    /// <returns>True if email was sent successfully</returns>
    Task<bool> SendEmailAsync(string to, string subject, string body);

    /// <summary>
    /// Validate email format
    /// </summary>
    /// <param name="email">Email to validate</param>
    /// <returns>True if valid</returns>
    bool IsValidEmail(string email);
}
