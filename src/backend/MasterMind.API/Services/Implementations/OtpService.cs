using System.Security.Cryptography;
using MasterMind.API.Data;
using MasterMind.API.Models.Entities;
using MasterMind.API.Models.Settings;
using MasterMind.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace MasterMind.API.Services.Implementations;

/// <summary>
/// OTP service implementation for generating and validating OTPs
/// </summary>
public class OtpService : IOtpService
{
    private readonly MasterMindDbContext _context;
    private readonly OtpSettings _settings;
    private readonly ISmsService _smsService;
    private readonly IEmailService _emailService;
    private readonly ILogger<OtpService> _logger;

    public OtpService(
        MasterMindDbContext context,
        IOptions<OtpSettings> settings,
        ISmsService smsService,
        IEmailService emailService,
        ILogger<OtpService> logger)
    {
        _context = context;
        _settings = settings.Value;
        _smsService = smsService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<string> GenerateOtpAsync(string identifier, OtpType type, OtpPurpose purpose, int? userId = null)
    {
        try
        {
            _logger.LogInformation("GenerateOtpAsync called for identifier: {Identifier}, type: {Type}, purpose: {Purpose}", identifier, type, purpose);
            
            // Check rate limiting
            var recentOtps = await _context.OtpRecords
                .Where(o => o.Identifier == identifier && 
                            o.CreatedAt > DateTime.UtcNow.AddHours(-1))
                .CountAsync();

            if (recentOtps >= _settings.MaxRequestsPerHour)
            {
                _logger.LogWarning("Rate limit exceeded for identifier: {Identifier}", identifier);
                throw new InvalidOperationException("Too many OTP requests. Please try again later.");
            }

            // Check cooldown period
            var lastOtp = await _context.OtpRecords
                .Where(o => o.Identifier == identifier && o.Purpose == purpose)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();

            if (lastOtp != null && 
                lastOtp.CreatedAt.AddSeconds(_settings.ResendCooldownSeconds) > DateTime.UtcNow)
            {
                var waitTime = (lastOtp.CreatedAt.AddSeconds(_settings.ResendCooldownSeconds) - DateTime.UtcNow).TotalSeconds;
                throw new InvalidOperationException($"Please wait {Math.Ceiling(waitTime)} seconds before requesting a new OTP.");
            }

            // Invalidate any existing OTPs for this identifier and purpose
            await InvalidateOtpAsync(identifier, purpose);

            // Generate new OTP
            var otp = GenerateRandomOtp();
            _logger.LogInformation("Generated OTP for {Identifier}", identifier);

            // Create OTP record
            var otpRecord = new OtpRecord
            {
                Identifier = identifier,
                OtpCode = HashOtp(otp), // Store hashed OTP for security
                Type = type,
                Purpose = purpose,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes),
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.OtpRecords.Add(otpRecord);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogWarning(dbEx, "EF insert for OtpRecord failed; attempting SQL fallback insert for {Identifier}", identifier);
                await InsertOtpRecordFallbackAsync(otpRecord);
            }
            _logger.LogInformation("OTP record saved to database for {Identifier}", identifier);

            // Send OTP via appropriate channel
            bool sent = type switch
            {
                OtpType.Mobile => await _smsService.SendOtpAsync(identifier, otp),
                OtpType.Email => await _emailService.SendOtpEmailAsync(identifier, otp),
                _ => false
            };

            if (!sent)
            {
                _logger.LogWarning("Failed to send OTP to {Identifier} via {Type}. Marking OTP as invalid.", identifier, type);
                otpRecord.IsUsed = true;
                otpRecord.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                throw new InvalidOperationException(type == OtpType.Email
                    ? "Unable to deliver OTP email right now. Please try again shortly."
                    : "Unable to deliver OTP SMS right now. Please try again shortly.");
            }

            _logger.LogInformation("OTP generated for {Identifier} ({Type}) for {Purpose}", identifier, type, purpose);
            
            return otp;
        }
        catch (InvalidOperationException)
        {
            throw; // Re-throw validation errors
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating OTP for {Identifier}. Inner: {InnerException}", identifier, ex.InnerException?.Message ?? "None");
            throw;
        }
    }

    public async Task<bool> ValidateOtpAsync(string identifier, string otp, OtpPurpose purpose)
    {
        var otpRecord = await _context.OtpRecords
            .Where(o => o.Identifier == identifier && 
                        o.Purpose == purpose && 
                        !o.IsUsed &&
                        o.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();

        if (otpRecord == null)
        {
            _logger.LogWarning("No valid OTP found for {Identifier}", identifier);
            return false;
        }

        // Check attempt count
        if (otpRecord.AttemptCount >= _settings.MaxAttempts)
        {
            _logger.LogWarning("Max OTP attempts exceeded for {Identifier}", identifier);
            otpRecord.IsUsed = true; // Mark as used to prevent further attempts
            await _context.SaveChangesAsync();
            return false;
        }

        // Increment attempt count
        otpRecord.AttemptCount++;

        // Verify OTP
        if (!VerifyOtp(otp, otpRecord.OtpCode))
        {
            _logger.LogWarning("Invalid OTP attempt for {Identifier}. Attempt {Attempt}/{Max}", 
                identifier, otpRecord.AttemptCount, _settings.MaxAttempts);
            await _context.SaveChangesAsync();
            return false;
        }

        // Mark OTP as used
        otpRecord.IsUsed = true;
        otpRecord.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("OTP validated successfully for {Identifier}", identifier);
        return true;
    }

    public async Task InvalidateOtpAsync(string identifier, OtpPurpose purpose)
    {
        var existingOtps = await _context.OtpRecords
            .Where(o => o.Identifier == identifier && 
                        o.Purpose == purpose && 
                        !o.IsUsed)
            .ToListAsync();

        foreach (var otp in existingOtps)
        {
            otp.IsUsed = true;
            otp.UpdatedAt = DateTime.UtcNow;
        }

        if (existingOtps.Any())
        {
            await _context.SaveChangesAsync();
            _logger.LogDebug("Invalidated {Count} existing OTPs for {Identifier}", existingOtps.Count, identifier);
        }
    }

    private string GenerateRandomOtp()
    {
        var bytes = new byte[4];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        
        var number = BitConverter.ToUInt32(bytes, 0);
        var maxValue = (uint)Math.Pow(10, _settings.Length);
        var otp = (number % maxValue).ToString().PadLeft(_settings.Length, '0');
        
        return otp;
    }

    private static string HashOtp(string otp)
    {
        // Use BCrypt for OTP hashing (same as passwords)
        return BCrypt.Net.BCrypt.HashPassword(otp, BCrypt.Net.BCrypt.GenerateSalt(10));
    }

    private static bool VerifyOtp(string otp, string hashedOtp)
    {
        return BCrypt.Net.BCrypt.Verify(otp, hashedOtp);
    }

    private async Task InsertOtpRecordFallbackAsync(OtpRecord otpRecord)
    {
        var provider = _context.Database.ProviderName ?? string.Empty;
        if (!provider.Contains("SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("OTP save failed and fallback is only available for SQL Server.");
        }

        var hasIsDeleted = false;
        var connection = _context.Database.GetDbConnection();
        var shouldClose = connection.State != System.Data.ConnectionState.Open;
        if (shouldClose)
        {
            await connection.OpenAsync();
        }

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = @"
SELECT COUNT(1)
FROM sys.columns c
JOIN sys.tables t ON c.object_id = t.object_id
WHERE t.name = 'OtpRecords' AND c.name = 'IsDeleted'";
            var result = await command.ExecuteScalarAsync();
            var count = Convert.ToInt32(result ?? 0);
            hasIsDeleted = count > 0;
        }
        finally
        {
            if (shouldClose)
            {
                await connection.CloseAsync();
            }
        }

        if (hasIsDeleted)
        {
            await _context.Database.ExecuteSqlRawAsync(@"
INSERT INTO dbo.OtpRecords (Identifier, OtpCode, Type, Purpose, ExpiresAt, IsUsed, AttemptCount, UserId, CreatedAt, UpdatedAt, IsDeleted)
VALUES (@identifier, @otpCode, @type, @purpose, @expiresAt, @isUsed, @attemptCount, @userId, @createdAt, @updatedAt, @isDeleted)",
                new SqlParameter("@identifier", otpRecord.Identifier),
                new SqlParameter("@otpCode", otpRecord.OtpCode),
                new SqlParameter("@type", (int)otpRecord.Type),
                new SqlParameter("@purpose", (int)otpRecord.Purpose),
                new SqlParameter("@expiresAt", otpRecord.ExpiresAt),
                new SqlParameter("@isUsed", otpRecord.IsUsed),
                new SqlParameter("@attemptCount", otpRecord.AttemptCount),
                new SqlParameter("@userId", (object?)otpRecord.UserId ?? DBNull.Value),
                new SqlParameter("@createdAt", otpRecord.CreatedAt),
                new SqlParameter("@updatedAt", (object?)otpRecord.UpdatedAt ?? DBNull.Value),
                new SqlParameter("@isDeleted", false));
        }
        else
        {
            await _context.Database.ExecuteSqlRawAsync(@"
INSERT INTO dbo.OtpRecords (Identifier, OtpCode, Type, Purpose, ExpiresAt, IsUsed, AttemptCount, UserId, CreatedAt, UpdatedAt)
VALUES (@identifier, @otpCode, @type, @purpose, @expiresAt, @isUsed, @attemptCount, @userId, @createdAt, @updatedAt)",
                new SqlParameter("@identifier", otpRecord.Identifier),
                new SqlParameter("@otpCode", otpRecord.OtpCode),
                new SqlParameter("@type", (int)otpRecord.Type),
                new SqlParameter("@purpose", (int)otpRecord.Purpose),
                new SqlParameter("@expiresAt", otpRecord.ExpiresAt),
                new SqlParameter("@isUsed", otpRecord.IsUsed),
                new SqlParameter("@attemptCount", otpRecord.AttemptCount),
                new SqlParameter("@userId", (object?)otpRecord.UserId ?? DBNull.Value),
                new SqlParameter("@createdAt", otpRecord.CreatedAt),
                new SqlParameter("@updatedAt", (object?)otpRecord.UpdatedAt ?? DBNull.Value));
        }
    }
}
