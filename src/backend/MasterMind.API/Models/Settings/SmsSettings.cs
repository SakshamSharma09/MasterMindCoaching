namespace MasterMind.API.Models.Settings;

/// <summary>
/// SMS provider configuration settings
/// Supports Fast2SMS and MSG91 providers
/// </summary>
public class SmsSettings
{
    public const string SectionName = "Sms";

    /// <summary>
    /// SMS provider to use: "Fast2SMS" or "MSG91"
    /// </summary>
    public string Provider { get; set; } = "Fast2SMS";

    /// <summary>
    /// API key for the SMS provider
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Sender ID (for MSG91)
    /// </summary>
    public string SenderId { get; set; } = "MSTMND";

    /// <summary>
    /// Template ID for OTP messages (for MSG91 DLT compliance)
    /// </summary>
    public string TemplateId { get; set; } = string.Empty;

    /// <summary>
    /// Route for Fast2SMS: "q" (quick), "dlt" (DLT), "otp" (OTP)
    /// </summary>
    public string Route { get; set; } = "otp";

    /// <summary>
    /// Whether to use sandbox/test mode
    /// </summary>
    public bool UseSandbox { get; set; } = true;

    /// <summary>
    /// Default country code for mobile numbers
    /// </summary>
    public string DefaultCountryCode { get; set; } = "91";

    /// <summary>
    /// OTP message template (use {otp} as placeholder)
    /// </summary>
    public string OtpMessageTemplate { get; set; } = "Your MasterMind Coaching verification code is {otp}. Valid for 5 minutes. Do not share with anyone.";
}
