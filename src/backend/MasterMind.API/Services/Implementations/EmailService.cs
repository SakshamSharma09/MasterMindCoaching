using System.Net;
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

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
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
            var emailSettings = _configuration.GetSection("Email");
            var smtpServer = emailSettings["SmtpServer"];
            var port = int.Parse(emailSettings["Port"] ?? "587");
            var useSsl = bool.Parse(emailSettings["UseSsl"] ?? "true");
            var username = emailSettings["Username"];
            var password = emailSettings["Password"];
            var fromEmail = emailSettings["FromEmail"];
            var fromName = emailSettings["FromName"];

            // If credentials are not configured, log and return (sandbox mode)
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                _logger.LogInformation("SANDBOX MODE - Email would be sent to {To}: {Subject}", to, subject);
                _logger.LogDebug("Email body: {Body}", body);
                return true;
            }

            using var client = new SmtpClient(smtpServer, port)
            {
                EnableSsl = useSsl,
                Credentials = new NetworkCredential(username, password)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail ?? username, fromName ?? "MasterMind Coaching"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(to);

            await client.SendMailAsync(mailMessage);
            
            _logger.LogInformation("Email sent successfully to {To}", to);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
            return false;
        }
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
