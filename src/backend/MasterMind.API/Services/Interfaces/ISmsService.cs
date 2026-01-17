namespace MasterMind.API.Services.Interfaces;

/// <summary>
/// SMS service interface for sending SMS messages
/// </summary>
public interface ISmsService
{
    /// <summary>
    /// Send an OTP SMS to the specified mobile number
    /// </summary>
    /// <param name="mobileNumber">Mobile number (with or without country code)</param>
    /// <param name="otp">OTP code to send</param>
    /// <returns>True if SMS was sent successfully</returns>
    Task<bool> SendOtpAsync(string mobileNumber, string otp);

    /// <summary>
    /// Send a custom SMS message
    /// </summary>
    /// <param name="mobileNumber">Mobile number</param>
    /// <param name="message">Message content</param>
    /// <returns>True if SMS was sent successfully</returns>
    Task<bool> SendSmsAsync(string mobileNumber, string message);

    /// <summary>
    /// Validate mobile number format
    /// </summary>
    /// <param name="mobileNumber">Mobile number to validate</param>
    /// <returns>True if valid</returns>
    bool IsValidMobileNumber(string mobileNumber);

    /// <summary>
    /// Format mobile number with country code
    /// </summary>
    /// <param name="mobileNumber">Mobile number</param>
    /// <returns>Formatted mobile number</returns>
    string FormatMobileNumber(string mobileNumber);
}
