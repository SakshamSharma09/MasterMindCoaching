namespace MasterMind.API.Models.Entities;

/// <summary>
/// Auditable request to delete an application account and its associated data.
/// </summary>
public class AccountDeletionRequest : BaseEntity
{
    public int? UserId { get; set; }
    public string EmailOrMobile { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime? CompletedAt { get; set; }

    public User? User { get; set; }
}
