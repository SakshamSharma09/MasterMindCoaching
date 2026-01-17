namespace MasterMind.API.Models.DTOs.Auth;

/// <summary>
/// DTO for user information
/// </summary>
public class UserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public bool IsActive { get; set; }
    public bool IsEmailVerified { get; set; }
    public bool IsMobileVerified { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? ProfileImageUrl { get; set; }
    public List<string> Roles { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}
