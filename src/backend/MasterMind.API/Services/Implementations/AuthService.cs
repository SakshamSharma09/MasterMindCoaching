using MasterMind.API.Data;
using MasterMind.API.Models.DTOs.Auth;
using MasterMind.API.Models.Entities;
using MasterMind.API.Models.Settings;
using MasterMind.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MasterMind.API.Services.Implementations;

/// <summary>
/// Authentication service implementation
/// </summary>
public class AuthService : IAuthService
{
    private readonly MasterMindDbContext _context;
    private readonly IOtpService _otpService;
    private readonly IJwtService _jwtService;
    private readonly IUserService _userService;
    private readonly ISmsService _smsService;
    private readonly IEmailService _emailService;
    private readonly OtpSettings _otpSettings;
    private readonly ILogger<AuthService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(
        MasterMindDbContext context,
        IOtpService otpService,
        IJwtService jwtService,
        IUserService userService,
        ISmsService smsService,
        IEmailService emailService,
        IOptions<OtpSettings> otpSettings,
        ILogger<AuthService> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _otpService = otpService;
        _jwtService = jwtService;
        _userService = userService;
        _smsService = smsService;
        _emailService = emailService;
        _otpSettings = otpSettings.Value;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<OtpResponseDto> RequestOtpAsync(OtpRequestDto request)
    {
        try
        {
            var isMobile = string.Equals(request.Type?.Trim(), "mobile", StringComparison.OrdinalIgnoreCase);
            var identifier = request.Identifier.Trim();
            if (!isMobile)
            {
                identifier = identifier.ToLowerInvariant();
            }

            // Validate identifier format
            if (isMobile && !_smsService.IsValidMobileNumber(identifier))
            {
                return new OtpResponseDto
                {
                    Success = false,
                    Message = "Invalid mobile number format"
                };
            }

            if (!isMobile && !_emailService.IsValidEmail(identifier))
            {
                return new OtpResponseDto
                {
                    Success = false,
                    Message = "Invalid email format"
                };
            }

            // Check if user exists
            var existingUser = await _userService.GetByIdentifierAsync(identifier);
            var isNewUser = existingUser == null;

            // Parse purpose (default to Login if not specified)
            var purpose = ParseOtpPurpose(request.Purpose);

            // Do not auto-create users during OTP request.
            // User creation is deferred to OTP verification.

            // For explicit registration, user must not exist
            if (purpose == OtpPurpose.Registration && !isNewUser)
            {
                return new OtpResponseDto
                {
                    Success = false,
                    Message = "User already exists. Please login instead."
                };
            }

            // Generate and send OTP
            var otpType = isMobile ? OtpType.Mobile : OtpType.Email;
            var otp = await _otpService.GenerateOtpAsync(
                identifier, 
                otpType, 
                purpose, 
                existingUser?.Id);

            _logger.LogInformation("OTP requested for {Identifier} ({Type}) - Purpose: {Purpose}", 
                MaskIdentifier(identifier, isMobile), request.Type, purpose);

            return new OtpResponseDto
            {
                Success = true,
                Message = $"OTP sent to your {(isMobile ? "mobile" : "email")}",
                MaskedIdentifier = MaskIdentifier(identifier, isMobile),
                ExpirySeconds = _otpSettings.ExpiryMinutes * 60,
                IsNewUser = isNewUser,
                RetryAfterSeconds = _otpSettings.ResendCooldownSeconds
            };
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "OTP request failed for {Identifier}", request.Identifier);
            return new OtpResponseDto
            {
                Success = false,
                Message = ex.Message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting OTP for {Identifier}. Inner exception: {InnerException}", 
                request.Identifier, ex.InnerException?.Message ?? "None");
            return new OtpResponseDto
            {
                Success = false,
                Message = $"An error occurred: {ex.Message}"
            };
        }
    }

    private static string GeneratePlaceholderMobile()
    {
        // 20-char max in schema; keep deterministic length and uniqueness.
        return $"E{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
    }

    private static string GeneratePlaceholderEmail()
    {
        return $"mobile_{Guid.NewGuid():N}@placeholder.mastermind.local";
    }

    private async Task<User?> TryProvisionLinkedUserAsync(string identifier, bool isMobile)
    {
        var normalizedIdentifier = identifier.Trim();
        var normalizedEmail = normalizedIdentifier.ToLowerInvariant();
        var normalizedMobile = NormalizeMobile(normalizedIdentifier);

        if (!isMobile)
        {
            var teacher = await _context.Teachers
                .Where(t => !t.IsDeleted && t.Email.ToLower() == normalizedEmail)
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefaultAsync();
            if (teacher != null)
            {
                return await EnsureTeacherLinkedUserAsync(teacher, normalizedEmail);
            }

            var students = await _context.Students
                .Where(s => !s.IsDeleted && s.ParentEmail != null && s.ParentEmail.ToLower() == normalizedEmail)
                .ToListAsync();
            if (students.Any())
            {
                return await EnsureParentLinkedUserAsync(students, normalizedEmail);
            }

            return null;
        }

        var teacherByMobile = (await _context.Teachers.Where(t => !t.IsDeleted).ToListAsync())
            .FirstOrDefault(t => !string.IsNullOrWhiteSpace(t.Mobile) && NormalizeMobile(t.Mobile) == normalizedMobile);
        if (teacherByMobile != null)
        {
            return await EnsureTeacherLinkedUserAsync(teacherByMobile, teacherByMobile.Email.ToLowerInvariant());
        }

        var parentStudents = (await _context.Students.Where(s => !s.IsDeleted).ToListAsync())
            .Where(s => !string.IsNullOrWhiteSpace(s.ParentMobile) && NormalizeMobile(s.ParentMobile) == normalizedMobile)
            .ToList();
        if (parentStudents.Any())
        {
            var parentEmail = parentStudents.FirstOrDefault(s => !string.IsNullOrWhiteSpace(s.ParentEmail))?.ParentEmail?.ToLowerInvariant();
            return await EnsureParentLinkedUserAsync(parentStudents, parentEmail);
        }

        return null;
    }

    private async Task<User> EnsureTeacherLinkedUserAsync(Teacher teacher, string email)
    {
        User? user = null;
        if (teacher.UserId.HasValue)
        {
            user = await _userService.GetByIdAsync(teacher.UserId.Value);
        }

        user ??= await _userService.GetByEmailAsync(email);

        if (user == null)
        {
            user = new User
            {
                Email = email,
                Mobile = string.IsNullOrWhiteSpace(teacher.Mobile) ? GeneratePlaceholderMobile() : teacher.Mobile,
                FirstName = teacher.FirstName,
                LastName = teacher.LastName,
                IsActive = true,
                IsEmailVerified = true,
                IsMobileVerified = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        await _userService.AssignRoleAsync(user.Id, "Teacher");

        if (teacher.UserId != user.Id)
        {
            teacher.UserId = user.Id;
            teacher.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return (await _userService.GetByIdAsync(user.Id))!;
    }

    private async Task<User> EnsureParentLinkedUserAsync(List<Student> students, string? email)
    {
        var parentEmail = string.IsNullOrWhiteSpace(email)
            ? $"{Guid.NewGuid():N}@placeholder.mastermind.local"
            : email.ToLowerInvariant();
        var primaryStudent = students[0];

        var existingParentUserId = students.Select(s => s.ParentUserId).FirstOrDefault(id => id.HasValue);
        User? user = null;
        if (existingParentUserId.HasValue)
        {
            user = await _userService.GetByIdAsync(existingParentUserId.Value);
        }

        user ??= await _userService.GetByEmailAsync(parentEmail);

        if (user == null)
        {
            var (firstName, lastName) = SplitName(primaryStudent.ParentName);
            user = new User
            {
                Email = parentEmail,
                Mobile = string.IsNullOrWhiteSpace(primaryStudent.ParentMobile) ? GeneratePlaceholderMobile() : primaryStudent.ParentMobile,
                FirstName = firstName,
                LastName = lastName,
                IsActive = true,
                IsEmailVerified = !parentEmail.Contains("@placeholder."),
                IsMobileVerified = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        await _userService.AssignRoleAsync(user.Id, "Parent");

        foreach (var student in students.Where(s => !s.ParentUserId.HasValue || s.ParentUserId == user.Id))
        {
            student.ParentUserId = user.Id;
            student.UpdatedAt = DateTime.UtcNow;
        }
        await _context.SaveChangesAsync();

        return (await _userService.GetByIdAsync(user.Id))!;
    }

    private static string NormalizeMobile(string mobile)
    {
        var digitsOnly = new string(mobile.Where(char.IsDigit).ToArray());
        if (digitsOnly.Length == 12 && digitsOnly.StartsWith("91"))
        {
            return digitsOnly.Substring(2);
        }
        return digitsOnly;
    }

    private static (string FirstName, string LastName) SplitName(string? fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return ("Parent", "User");
        }

        var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1)
        {
            return (parts[0], "");
        }
        return (parts[0], string.Join(' ', parts.Skip(1)));
    }

    public async Task<AuthResponseDto> VerifyOtpAsync(OtpVerifyDto request)
    {
        try
        {
            var identifier = request.Identifier.Trim();
            var purpose = ParseOtpPurpose(request.Purpose);

            // Validate OTP
            var isValid = await _otpService.ValidateOtpAsync(identifier, request.Otp, purpose);
            if (!isValid)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid or expired OTP",
                    ErrorCode = "INVALID_OTP"
                };
            }

            // Get or create user
            var user = await _userService.GetByIdentifierAsync(identifier);
            var isMobile = !identifier.Contains('@');

            if (user == null)
            {
                if (purpose == OtpPurpose.Registration)
                {
                    if (request.RegistrationDetails == null)
                    {
                        return new AuthResponseDto
                        {
                            Success = false,
                            Message = "Registration details are required",
                            ErrorCode = "REGISTRATION_DETAILS_REQUIRED"
                        };
                    }

                    user = await _userService.CreateUserAsync(request.RegistrationDetails, identifier, isMobile);
                }
                else
                {
                    user = await TryProvisionLinkedUserAsync(identifier, isMobile);
                    if (user == null)
                    {
                        return new AuthResponseDto
                        {
                            Success = false,
                            Message = "Account is not provisioned. Ask admin to link this email/mobile to a Teacher or Student Parent record.",
                            ErrorCode = "ACCOUNT_NOT_PROVISIONED"
                        };
                    }
                }
            }
            else
            {
                // Update verification status
                if (isMobile && !user.IsMobileVerified)
                {
                    await _userService.VerifyMobileAsync(user.Id);
                }
                else if (!isMobile && !user.IsEmailVerified)
                {
                    await _userService.VerifyEmailAsync(user.Id);
                }
            }

            // Check if user is active
            if (!user.IsActive)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Your account has been deactivated. Please contact support.",
                    ErrorCode = "ACCOUNT_DEACTIVATED"
                };
            }

            // Update last login
            await _userService.UpdateLastLoginAsync(user.Id);

            // Generate tokens
            var roles = await _userService.GetUserRolesAsync(user.Id);
            var accessToken = _jwtService.GenerateAccessToken(user, roles);
            var refreshToken = _jwtService.GenerateRefreshToken(GetClientIp());

            // Save refresh token
            refreshToken.UserId = user.Id;
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            var userDto = await _userService.ToUserDtoAsync(user);

            _logger.LogInformation("User {UserId} authenticated successfully via OTP", user.Id);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Authentication successful",
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                ExpiresAt = _jwtService.GetAccessTokenExpiry(),
                User = userDto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying OTP for {Identifier}", request.Identifier);
            return new AuthResponseDto
            {
                Success = false,
                Message = "An error occurred during authentication",
                ErrorCode = "AUTH_ERROR"
            };
        }
    }

    public async Task<AuthResponseDto> LoginWithPasswordAsync(PasswordLoginDto request)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == request.Email && !u.IsDeleted);

            if (user == null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid email or password",
                    ErrorCode = "INVALID_CREDENTIALS"
                };
            }

            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Password not set. Please use OTP login or contact admin to set a password.",
                    ErrorCode = "PASSWORD_NOT_SET"
                };
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid email or password",
                    ErrorCode = "INVALID_CREDENTIALS"
                };
            }

            if (!user.IsActive)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Your account has been deactivated.",
                    ErrorCode = "ACCOUNT_DEACTIVATED"
                };
            }

            // Check user is Admin
            var isAdmin = user.UserRoles.Any(ur => ur.Role.Name == "Admin");
            if (!isAdmin)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Password login is only available for admin users. Please use OTP login.",
                    ErrorCode = "NOT_ADMIN"
                };
            }

            await _userService.UpdateLastLoginAsync(user.Id);

            var roles = user.UserRoles.Select(ur => ur.Role.Name);
            var accessToken = _jwtService.GenerateAccessToken(user, roles);
            var refreshToken = _jwtService.GenerateRefreshToken(GetClientIp());
            refreshToken.UserId = user.Id;
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            var userDto = await _userService.ToUserDtoAsync(user);

            _logger.LogInformation("Admin {UserId} authenticated via password", user.Id);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Authentication successful",
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                ExpiresAt = _jwtService.GetAccessTokenExpiry(),
                User = userDto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password login for {Email}", request.Email);
            return new AuthResponseDto
            {
                Success = false,
                Message = "An error occurred during authentication",
                ErrorCode = "AUTH_ERROR"
            };
        }
    }

    public async Task<AuthResponseDto> QuickLoginAsync(string email)
    {
        try
        {
            var allowedEmails = new[] { "admin@mastermind.com", "teacher@mastermind.com", "parent@mastermind.com" };
            var normalizedEmail = email.ToLower();
            
            if (!allowedEmails.Contains(normalizedEmail))
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid quick access email",
                    ErrorCode = "INVALID_EMAIL"
                };
            }

            // Determine role based on email
            var roleName = normalizedEmail switch
            {
                "admin@mastermind.com" => "Admin",
                "teacher@mastermind.com" => "Teacher",
                "parent@mastermind.com" => "Parent",
                _ => "Admin"
            };

            // Find or create the user
            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == normalizedEmail && !u.IsDeleted);

            if (user == null)
            {
                // Create the demo user
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
                if (role == null)
                {
                    role = new Role { Name = roleName, Description = $"{roleName} role" };
                    _context.Roles.Add(role);
                    await _context.SaveChangesAsync();
                }

                user = new User
                {
                    Email = normalizedEmail,
                    FirstName = roleName == "Admin" ? "Admin" : (roleName == "Teacher" ? "Test" : "Parent"),
                    LastName = roleName == "Parent" ? "User" : "User",
                    Mobile = "+1234567890",
                    IsActive = true,
                    IsEmailVerified = true,
                    IsMobileVerified = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Add role
                var userRole = new UserRole { UserId = user.Id, RoleId = role.Id };
                _context.UserRoles.Add(userRole);
                await _context.SaveChangesAsync();

                // Reload with roles
                user = await _context.Users
                    .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                    .FirstAsync(u => u.Id == user.Id);
            }

            if (!user.IsActive)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Your account has been deactivated.",
                    ErrorCode = "ACCOUNT_DEACTIVATED"
                };
            }

            await _userService.UpdateLastLoginAsync(user.Id);

            var roles = user.UserRoles.Select(ur => ur.Role.Name);
            var accessToken = _jwtService.GenerateAccessToken(user, roles);
            var refreshToken = _jwtService.GenerateRefreshToken(GetClientIp());
            refreshToken.UserId = user.Id;
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            var userDto = await _userService.ToUserDtoAsync(user);

            _logger.LogInformation("User {UserId} authenticated via quick login", user.Id);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Quick login successful",
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                ExpiresAt = _jwtService.GetAccessTokenExpiry(),
                User = userDto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during quick login for {Email}", email);
            return new AuthResponseDto
            {
                Success = false,
                Message = "An error occurred during quick login",
                ErrorCode = "AUTH_ERROR"
            };
        }
    }

    public async Task<AuthResponseDto> SetPasswordAsync(int userId, SetPasswordDto request)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

            if (user == null)
            {
                return new AuthResponseDto { Success = false, Message = "User not found", ErrorCode = "USER_NOT_FOUND" };
            }

            var isAdmin = user.UserRoles.Any(ur => ur.Role.Name == "Admin");
            if (!isAdmin)
            {
                return new AuthResponseDto { Success = false, Message = "Only admin users can set a password", ErrorCode = "NOT_ADMIN" };
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Password set for admin user {UserId}", userId);

            return new AuthResponseDto { Success = true, Message = "Password set successfully" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting password for user {UserId}", userId);
            return new AuthResponseDto { Success = false, Message = "An error occurred", ErrorCode = "SET_PASSWORD_ERROR" };
        }
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto request)
    {
        try
        {
            var refreshToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                    .ThenInclude(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

            if (refreshToken == null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid refresh token",
                    ErrorCode = "INVALID_REFRESH_TOKEN"
                };
            }

            if (!refreshToken.IsActive)
            {
                // Token has been revoked or expired - revoke all tokens for this user
                await RevokeAllUserTokensAsync(refreshToken.UserId, "Attempted use of revoked token");
                
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Refresh token has expired or been revoked",
                    ErrorCode = "TOKEN_EXPIRED"
                };
            }

            var user = refreshToken.User;
            if (!user.IsActive)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Account has been deactivated",
                    ErrorCode = "ACCOUNT_DEACTIVATED"
                };
            }

            // Rotate refresh token
            var newRefreshToken = _jwtService.GenerateRefreshToken(GetClientIp());
            newRefreshToken.UserId = user.Id;

            // Revoke old token
            refreshToken.IsRevoked = true;
            refreshToken.RevokedAt = DateTime.UtcNow;
            refreshToken.RevokedByIp = GetClientIp();
            refreshToken.ReplacedByToken = newRefreshToken.Token;
            refreshToken.ReasonRevoked = "Replaced by new token";

            _context.RefreshTokens.Add(newRefreshToken);
            await _context.SaveChangesAsync();

            // Generate new access token
            var roles = user.UserRoles.Select(ur => ur.Role.Name);
            var accessToken = _jwtService.GenerateAccessToken(user, roles);
            var userDto = await _userService.ToUserDtoAsync(user);

            _logger.LogInformation("Tokens refreshed for user {UserId}", user.Id);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Tokens refreshed successfully",
                AccessToken = accessToken,
                RefreshToken = newRefreshToken.Token,
                ExpiresAt = _jwtService.GetAccessTokenExpiry(),
                User = userDto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return new AuthResponseDto
            {
                Success = false,
                Message = "An error occurred while refreshing token",
                ErrorCode = "REFRESH_ERROR"
            };
        }
    }

    public async Task<bool> LogoutAsync(int userId, string? refreshToken = null)
    {
        try
        {
            if (!string.IsNullOrEmpty(refreshToken))
            {
                // Revoke specific token
                var token = await _context.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.UserId == userId);

                if (token != null)
                {
                    token.IsRevoked = true;
                    token.RevokedAt = DateTime.UtcNow;
                    token.RevokedByIp = GetClientIp();
                    token.ReasonRevoked = "User logout";
                }
            }
            else
            {
                // Revoke all tokens for user
                await RevokeAllUserTokensAsync(userId, "User logout (all devices)");
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("User {UserId} logged out", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout for user {UserId}", userId);
            return false;
        }
    }

    public async Task<UserDto?> GetCurrentUserAsync(int userId)
    {
        var user = await _userService.GetByIdAsync(userId);
        if (user == null) return null;
        return await _userService.ToUserDtoAsync(user);
    }

    private async Task RevokeAllUserTokensAsync(int userId, string reason)
    {
        var activeTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

        foreach (var token in activeTokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedByIp = GetClientIp();
            token.ReasonRevoked = reason;
        }
    }

    private static OtpPurpose ParseOtpPurpose(string purpose)
    {
        return purpose.ToLower() switch
        {
            "login" => OtpPurpose.Login,
            "registration" or "register" => OtpPurpose.Registration,
            "password_reset" or "reset" => OtpPurpose.PasswordReset,
            "email_verification" or "verify_email" => OtpPurpose.EmailVerification,
            "mobile_verification" or "verify_mobile" => OtpPurpose.MobileVerification,
            _ => OtpPurpose.Login
        };
    }

    private static string MaskIdentifier(string identifier, bool isMobile)
    {
        if (isMobile)
        {
            // Mask mobile: show last 4 digits
            var digits = new string(identifier.Where(char.IsDigit).ToArray());
            if (digits.Length >= 4)
            {
                return "****" + digits.Substring(digits.Length - 4);
            }
        }
        else
        {
            // Mask email: show first 2 chars and domain
            var parts = identifier.Split('@');
            if (parts.Length == 2)
            {
                var localPart = parts[0];
                var domain = parts[1];
                var maskedLocal = localPart.Length > 2 
                    ? localPart.Substring(0, 2) + "***" 
                    : localPart + "***";
                return maskedLocal + "@" + domain;
            }
        }
        return "****";
    }

    private string GetClientIp()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return "unknown";

        // Check for forwarded IP (behind proxy/load balancer)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
