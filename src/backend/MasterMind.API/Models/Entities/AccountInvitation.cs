namespace MasterMind.API.Models.Entities;

/// <summary>
/// Single-use invitation that lets an institution-provisioned user set a password.
/// Only a SHA-256 hash of the raw token is persisted.
/// </summary>
public class AccountInvitation : BaseEntity
{
    public int UserId { get; set; }
    public int? StudentId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public int? CreatedByUserId { get; set; }

    public User User { get; set; } = null!;
    public Student? Student { get; set; }
    public User? CreatedByUser { get; set; }
}
