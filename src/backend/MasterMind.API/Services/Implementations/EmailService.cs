using System.Net;
using System.Net.Http.Json;
using System.Net.Mail;
using System.Text.RegularExpressions;
using MasterMind.API.Services.Interfaces;

namespace MasterMind.API.Services.Implementations;

/// <summary>
/// Email service implementation for sending emails via SMTP
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly HttpClient _httpClient;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger, HttpClient httpClient)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<bool> SendOtpEmailAsync(string email, string otp)
    {
        var subject = "Your MasterMind Coaching Verification Code";
        var body = GetOtpEmailTemplate(otp);
        return await SendEmailAsync(email, subject, body);
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            _logger.LogInformation("EmailService: Starting email send to {To}", to);
            
            var emailSettings = _configuration.GetSection("Email");
            if (!IsValidEmail(to))
            {
                _logger.LogWarning("EmailService: Invalid recipient email address");
                return false;
            }

            var provider = emailSettings["Provider"];
            var apiKey = emailSettings["ApiKey"];
            var smtpServer = emailSettings["SmtpServer"];
            var port = int.Parse(emailSettings["Port"] ?? "587");
            var useSsl = bool.Parse(emailSettings["UseSsl"] ?? "true");
            var useSandbox = bool.Parse(emailSettings["UseSandbox"] ?? "false");
            var username = emailSettings["Username"];
            var password = emailSettings["Password"];
            var fromEmail = emailSettings["FromEmail"];
            var fromName = emailSettings["FromName"];

            _logger.LogInformation("EmailService: Provider: {Provider}, SMTP Server: {SmtpServer}, Port: {Port}, UseSsl: {UseSsl}, Sandbox: {Sandbox}",
                provider, smtpServer, port, useSsl, useSandbox);

            // If sandbox mode, just log and return (for testing)
            if (useSandbox)
            {
                _logger.LogInformation("SANDBOX MODE - Email would be sent to {To}: {Subject}", to, subject);
                return true;
            }

            var useSendGrid = string.Equals(provider, "SendGrid", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(apiKey);

            if (useSendGrid)
            {
                return await SendWithSendGridAsync(apiKey, fromEmail, fromName, to, subject, body);
            }

            if (string.Equals(provider, "SendGrid", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("EmailService: Provider is SendGrid but ApiKey is missing. Falling back to SMTP if credentials are configured.");
            }

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                _logger.LogError("EmailService: SMTP credentials are not configured. Enable Email:UseSandbox for local testing or provide credentials.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(smtpServer))
            {
                _logger.LogError("EmailService: SMTP server is not configured.");
                return false;
            }

            using var client = new SmtpClient(smtpServer, port)
            {
                EnableSsl = useSsl,
                Credentials = new NetworkCredential(username, password),
                Timeout = 30000 // 30 second timeout
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail ?? username, fromName ?? "MasterMind Coaching"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(to);

            _logger.LogInformation("EmailService: Sending email via SMTP...");
            await client.SendMailAsync(mailMessage);
            
            _logger.LogInformation("Email sent successfully to {To}", to);
            return true;
        }
        catch (SmtpException smtpEx)
        {
            _logger.LogError(smtpEx, "SMTP Error sending email to {To}. Status: {Status}, Message: {Message}", 
                to, smtpEx.StatusCode, smtpEx.Message);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
            return false;
        }
    }

    private async Task<bool> SendWithSendGridAsync(string? apiKey, string? fromEmail, string? fromName, string to, string subject, string body)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogError("EmailService: SendGrid API key is not configured. OTP email was not sent.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(fromEmail))
        {
            _logger.LogError("EmailService: FromEmail is required for SendGrid delivery.");
            return false;
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.sendgrid.com/v3/mail/send");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = JsonContent.Create(new
        {
            personalizations = new[]
            {
                new
                {
                    to = new[] { new { email = to } }
                }
            },
            from = new { email = fromEmail, name = fromName ?? "MasterMind Coaching" },
            subject,
            content = new[]
            {
                new { type = "text/html", value = body }
            }
        });

        var response = await _httpClient.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("Email sent successfully to {To} through SendGrid", to);
            return true;
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        _logger.LogError("SendGrid email failed for {To}. Status: {StatusCode}, Body: {Body}", to, response.StatusCode, responseBody);
        return false;
    }

    public bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            // Use regex for basic validation
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
            return emailRegex.IsMatch(email);
        }
        catch
        {
            return false;
        }
    }

    private static string GetOtpEmailTemplate(string otp)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Verification Code</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
        <h1 style='color: white; margin: 0; font-size: 28px;'>MasterMind Coaching</h1>
        <p style='color: rgba(255,255,255,0.9); margin: 10px 0 0 0;'>Your Learning Partner</p>
    </div>
    
    <div style='background: #ffffff; padding: 40px 30px; border: 1px solid #e0e0e0; border-top: none;'>
        <h2 style='color: #333; margin-top: 0;'>Verification Code</h2>
        <p>Hello,</p>
        <p>Your verification code for MasterMind Coaching is:</p>
        
        <div style='background: #f8f9fa; border: 2px dashed #667eea; border-radius: 10px; padding: 20px; text-align: center; margin: 30px 0;'>
            <span style='font-size: 36px; font-weight: bold; letter-spacing: 8px; color: #667eea;'>{otp}</span>
        </div>
        
        <p style='color: #666;'>This code will expire in <strong>5 minutes</strong>.</p>
        <p style='color: #666;'>If you didn't request this code, please ignore this email or contact support if you have concerns.</p>
        
        <hr style='border: none; border-top: 1px solid #e0e0e0; margin: 30px 0;'>
        
        <p style='color: #999; font-size: 12px; margin-bottom: 0;'>
            This is an automated message. Please do not reply to this email.
        </p>
    </div>
    
    <div style='background: #f8f9fa; padding: 20px; text-align: center; border-radius: 0 0 10px 10px; border: 1px solid #e0e0e0; border-top: none;'>
        <p style='color: #666; font-size: 12px; margin: 0;'>
            © {DateTime.Now.Year} MasterMind Coaching Classes. All rights reserved.
        </p>
    </div>
</body>
</html>";
    }
}
