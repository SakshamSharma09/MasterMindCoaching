using MasterMind.API.Models.DTOs.Auth;
using MasterMind.API.Models.Entities;

namespace MasterMind.API.Services.Interfaces;

/// <summary>
/// User service interface for user management
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Get user by ID
    /// </summary>
    Task<User?> GetByIdAsync(int id);

    /// <summary>
    /// Get user by email
    /// </summary>
    Task<User?> GetByEmailAsync(string email);

    /// <summary>
    /// Get user by mobile number
    /// </summary>
    Task<User?> GetByMobileAsync(string mobile);

    /// <summary>
    /// Get user by email or mobile
    /// </summary>
    Task<User?> GetByIdentifierAsync(string identifier);

    /// <summary>
    /// Create a new user
    /// </summary>
    Task<User> CreateUserAsync(RegistrationDetailsDto details, string identifier, bool isMobile);

    /// <summary>
    /// Update user's last login time
    /// </summary>
    Task UpdateLastLoginAsync(int userId);

    /// <summary>
    /// Verify user's mobile number
    /// </summary>
    Task VerifyMobileAsync(int userId);

    /// <summary>
    /// Verify user's email
    /// </summary>
    Task VerifyEmailAsync(int userId);

    /// <summary>
    /// Get user roles
    /// </summary>
    Task<IEnumerable<string>> GetUserRolesAsync(int userId);

    /// <summary>
    /// Assign role to user
    /// </summary>
    Task AssignRoleAsync(int userId, string roleName);

    /// <summary>
    /// Check if user exists by identifier
    /// </summary>
    Task<bool> ExistsAsync(string identifier);

    /// <summary>
    /// Convert User entity to UserDto
    /// </summary>
    Task<UserDto> ToUserDtoAsync(User user);
}
