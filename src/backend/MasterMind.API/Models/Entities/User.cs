namespace MasterMind.API.Models.Entities;

/// <summary>
/// User entity for authentication and authorization
/// </summary>
public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsEmailVerified { get; set; } = false;
    public bool IsMobileVerified { get; set; } = false;
    public DateTime? LastLoginAt { get; set; }
    public string? ProfileImageUrl { get; set; }

    // Navigation properties
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
