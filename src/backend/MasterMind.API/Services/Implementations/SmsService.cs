using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using MasterMind.API.Models.Settings;
using MasterMind.API.Services.Interfaces;
using Microsoft.Extensions.Options;
using Serilog;

namespace MasterMind.API.Services.Implementations;

/// <summary>
/// SMS service implementation supporting Fast2SMS and MSG91 providers
/// </summary>
public class SmsService : ISmsService
{
    private readonly SmsSettings _settings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<SmsService> _logger;

    // Fast2SMS API endpoints
    private const string Fast2SmsOtpUrl = "https://www.fast2sms.com/dev/bulkV2";
    
    // MSG91 API endpoints
    private const string Msg91OtpUrl = "https://api.msg91.com/api/v5/otp";
    private const string Msg91SmsUrl = "https://api.msg91.com/api/v5/flow/";

    public SmsService(
        IOptions<SmsSettings> settings,
        HttpClient httpClient,
        ILogger<SmsService> logger)
    {
        _settings = settings.Value;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<bool> SendOtpAsync(string mobileNumber, string otp)
    {
        try
        {
            var formattedNumber = FormatMobileNumber(mobileNumber);
            
            if (!IsValidMobileNumber(formattedNumber))
            {
                _logger.LogWarning("Invalid mobile number format: {MobileNumber}", mobileNumber);
                return false;
            }

            // In sandbox mode, just log and return success
            if (_settings.UseSandbox)
            {
                _logger.LogInformation("SANDBOX MODE - OTP {Otp} would be sent to {Mobile}", otp, formattedNumber);
                return true;
            }

            return _settings.Provider.ToLower() switch
            {
                "fast2sms" => await SendViaFast2SmsAsync(formattedNumber, otp),
                "msg91" => await SendViaMsg91Async(formattedNumber, otp),
                _ => throw new InvalidOperationException($"Unknown SMS provider: {_settings.Provider}")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send OTP to {MobileNumber}", mobileNumber);
            return false;
        }
    }

    public async Task<bool> SendSmsAsync(string mobileNumber, string message)
    {
        try
        {
            var formattedNumber = FormatMobileNumber(mobileNumber);
            
            if (!IsValidMobileNumber(formattedNumber))
            {
                _logger.LogWarning("Invalid mobile number format: {MobileNumber}", mobileNumber);
                return false;
            }

            if (_settings.UseSandbox)
            {
                _logger.LogInformation("SANDBOX MODE - SMS would be sent to {Mobile}: {Message}", formattedNumber, message);
                return true;
            }

            // For custom SMS, use Fast2SMS quick route or MSG91 flow
            return _settings.Provider.ToLower() switch
            {
                "fast2sms" => await SendCustomViaFast2SmsAsync(formattedNumber, message),
                "msg91" => await SendCustomViaMsg91Async(formattedNumber, message),
                _ => throw new InvalidOperationException($"Unknown SMS provider: {_settings.Provider}")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {MobileNumber}", mobileNumber);
            return false;
        }
    }

    public bool IsValidMobileNumber(string mobileNumber)
    {
        if (string.IsNullOrWhiteSpace(mobileNumber))
            return false;

        // Remove any non-digit characters for validation
        var digitsOnly = Regex.Replace(mobileNumber, @"\D", "");
        
        // Indian mobile numbers: 10 digits starting with 6-9
        // With country code: 12 digits starting with 91
        if (digitsOnly.Length == 10)
        {
            return Regex.IsMatch(digitsOnly, @"^[6-9]\d{9}$");
        }
        else if (digitsOnly.Length == 12 && digitsOnly.StartsWith("91"))
        {
            return Regex.IsMatch(digitsOnly.Substring(2), @"^[6-9]\d{9}$");
        }

        return false;
    }

    public string FormatMobileNumber(string mobileNumber)
    {
        // Remove all non-digit characters
        var digitsOnly = Regex.Replace(mobileNumber, @"\D", "");
        
        // If 10 digits, add country code
        if (digitsOnly.Length == 10)
        {
            return _settings.DefaultCountryCode + digitsOnly;
        }
        
        // If already has country code, return as is
        if (digitsOnly.Length == 12 && digitsOnly.StartsWith(_settings.DefaultCountryCode))
        {
            return digitsOnly;
        }

        // Return original digits if format is unexpected
        return digitsOnly;
    }

    private async Task<bool> SendViaFast2SmsAsync(string mobileNumber, string otp)
    {
        try
        {
            // Remove country code for Fast2SMS (they expect 10 digit numbers)
            var number = mobileNumber.Length == 12 ? mobileNumber.Substring(2) : mobileNumber;
            
            var message = _settings.OtpMessageTemplate.Replace("{otp}", otp);

            var requestData = new Dictionary<string, string>
            {
                { "route", _settings.Route },
                { "message", message },
                { "language", "english" },
                { "flash", "0" },
                { "numbers", number }
            };

            // For OTP route, use variables
            if (_settings.Route == "otp")
            {
                requestData = new Dictionary<string, string>
                {
                    { "route", "otp" },
                    { "variables_values", otp },
                    { "numbers", number }
                };
            }

            var request = new HttpRequestMessage(HttpMethod.Post, Fast2SmsOtpUrl);
            request.Headers.Add("authorization", _settings.ApiKey);
            request.Content = new FormUrlEncodedContent(requestData);

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogDebug("Fast2SMS Response: {Response}", responseContent);

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<Fast2SmsResponse>(responseContent);
                if (result?.Return == true)
                {
                    _logger.LogInformation("OTP sent successfully via Fast2SMS to {Mobile}", number);
                    return true;
                }
            }

            _logger.LogWarning("Fast2SMS failed: {Response}", responseContent);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fast2SMS API error");
            return false;
        }
    }

    private async Task<bool> SendViaMsg91Async(string mobileNumber, string otp)
    {
        try
        {
            var requestData = new
            {
                template_id = _settings.TemplateId,
                mobile = mobileNumber,
                otp = otp
            };

            var request = new HttpRequestMessage(HttpMethod.Post, Msg91OtpUrl);
            request.Headers.Add("authkey", _settings.ApiKey);
            request.Content = new StringContent(
                JsonSerializer.Serialize(requestData),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogDebug("MSG91 Response: {Response}", responseContent);

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<Msg91Response>(responseContent);
                if (result?.Type == "success")
                {
                    _logger.LogInformation("OTP sent successfully via MSG91 to {Mobile}", mobileNumber);
                    return true;
                }
            }

            _logger.LogWarning("MSG91 failed: {Response}", responseContent);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MSG91 API error");
            return false;
        }
    }

    private async Task<bool> SendCustomViaFast2SmsAsync(string mobileNumber, string message)
    {
        try
        {
            var number = mobileNumber.Length == 12 ? mobileNumber.Substring(2) : mobileNumber;

            var requestData = new Dictionary<string, string>
            {
                { "route", "q" }, // Quick route for custom messages
                { "message", message },
                { "language", "english" },
                { "flash", "0" },
                { "numbers", number }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, Fast2SmsOtpUrl);
            request.Headers.Add("authorization", _settings.ApiKey);
            request.Content = new FormUrlEncodedContent(requestData);

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<Fast2SmsResponse>(responseContent);
                return result?.Return == true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fast2SMS custom SMS error");
            return false;
        }
    }

    private async Task<bool> SendCustomViaMsg91Async(string mobileNumber, string message)
    {
        try
        {
            // MSG91 requires flow/template for transactional SMS
            // This is a simplified implementation
            var requestData = new
            {
                sender = _settings.SenderId,
                route = "4", // Transactional route
                country = _settings.DefaultCountryCode,
                sms = new[]
                {
                    new
                    {
                        message = message,
                        to = new[] { mobileNumber }
                    }
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.msg91.com/api/v2/sendsms");
            request.Headers.Add("authkey", _settings.ApiKey);
            request.Content = new StringContent(
                JsonSerializer.Serialize(requestData),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MSG91 custom SMS error");
            return false;
        }
    }

    // Response models for API responses
    private class Fast2SmsResponse
    {
        public bool Return { get; set; }
        public string? Message { get; set; }
        public string? Request_id { get; set; }
    }

    private class Msg91Response
    {
        public string? Type { get; set; }
        public string? Message { get; set; }
        public string? Request_id { get; set; }
    }
}
