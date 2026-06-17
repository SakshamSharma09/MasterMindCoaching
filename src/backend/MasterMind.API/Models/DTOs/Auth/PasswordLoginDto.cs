using System.ComponentModel.DataAnnotations;

namespace MasterMind.API.Models.DTOs.Auth;

public class PasswordLoginDto
{
    [EmailAddress]
    public string? Email { get; set; }

    public string? Mobile { get; set; }

    public string? Identifier { get; set; }

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
}

public class SetPasswordDto
{
    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare("Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
