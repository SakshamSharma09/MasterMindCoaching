using MasterMind.API.Data;
using MasterMind.API.Models.DTOs.Auth;
using MasterMind.API.Models.Entities;
using MasterMind.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MasterMind.API.Services.Implementations;

/// <summary>
/// User service implementation for user management
/// </summary>
public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserService> _logger;

    public UserService(ApplicationDbContext context, ILogger<UserService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && !u.IsDeleted);
    }

    public async Task<User?> GetByMobileAsync(string mobile)
    {
        // Normalize mobile number (remove country code if present)
        var normalizedMobile = NormalizeMobile(mobile);
        
        return await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Mobile == normalizedMobile && !u.IsDeleted);
    }

    public async Task<User?> GetByIdentifierAsync(string identifier)
    {
        // Check if identifier is email or mobile
        if (identifier.Contains('@'))
        {
            return await GetByEmailAsync(identifier);
        }
        return await GetByMobileAsync(identifier);
    }

    public async Task<User> CreateUserAsync(RegistrationDetailsDto details, string identifier, bool isMobile)
    {
        var user = new User
        {
            FirstName = details.FirstName,
            LastName = details.LastName,
            Email = isMobile ? (details.Email ?? string.Empty) : identifier,
            Mobile = isMobile ? NormalizeMobile(identifier) : (details.Mobile ?? string.Empty),
            IsEmailVerified = !isMobile && !string.IsNullOrEmpty(identifier),
            IsMobileVerified = isMobile,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Assign default role
        var roleName = details.Role ?? "Parent";
        await AssignRoleAsync(user.Id, roleName);

        _logger.LogInformation("Created new user {UserId} with role {Role}", user.Id, roleName);

        // Reload user with roles
        return (await GetByIdAsync(user.Id))!;
    }

    public async Task UpdateLastLoginAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.LastLoginAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task VerifyMobileAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.IsMobileVerified = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Mobile verified for user {UserId}", userId);
        }
    }

    public async Task VerifyEmailAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.IsEmailVerified = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Email verified for user {UserId}", userId);
        }
    }

    public async Task<IEnumerable<string>> GetUserRolesAsync(int userId)
    {
        return await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Include(ur => ur.Role)
            .Select(ur => ur.Role.Name)
            .ToListAsync();
    }

    public async Task AssignRoleAsync(int userId, string roleName)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
        if (role == null)
        {
            _logger.LogWarning("Role {RoleName} not found", roleName);
            throw new InvalidOperationException($"Role '{roleName}' not found");
        }

        var existingUserRole = await _context.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == role.Id);

        if (existingUserRole == null)
        {
            var userRole = new UserRole
            {
                UserId = userId,
                RoleId = role.Id,
                AssignedAt = DateTime.UtcNow
            };
            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Assigned role {RoleName} to user {UserId}", roleName, userId);
        }
    }

    public async Task<bool> ExistsAsync(string identifier)
    {
        var user = await GetByIdentifierAsync(identifier);
        return user != null;
    }

    public async Task<UserDto> ToUserDtoAsync(User user)
    {
        var roles = await GetUserRolesAsync(user.Id);
        
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Mobile = user.Mobile,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.IsActive,
            IsEmailVerified = user.IsEmailVerified,
            IsMobileVerified = user.IsMobileVerified,
            LastLoginAt = user.LastLoginAt,
            ProfileImageUrl = user.ProfileImageUrl,
            Roles = roles.ToList(),
            CreatedAt = user.CreatedAt
        };
    }

    private static string NormalizeMobile(string mobile)
    {
        // Remove all non-digit characters
        var digitsOnly = new string(mobile.Where(char.IsDigit).ToArray());
        
        // If 12 digits starting with 91, remove country code
        if (digitsOnly.Length == 12 && digitsOnly.StartsWith("91"))
        {
            return digitsOnly.Substring(2);
        }
        
        // Return 10 digit number
        return digitsOnly.Length == 10 ? digitsOnly : mobile;
    }
}
