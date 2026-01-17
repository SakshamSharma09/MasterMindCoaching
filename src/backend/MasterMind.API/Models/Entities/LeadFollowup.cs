namespace MasterMind.API.Models.Entities;

/// <summary>
/// Lead followup entity for tracking lead interactions
/// </summary>
public class LeadFollowup : BaseEntity
{
    public int LeadId { get; set; }
    public DateTime FollowupDate { get; set; } = DateTime.UtcNow;
    public FollowupType Type { get; set; }
    public string Notes { get; set; } = string.Empty;
    public LeadStatus? StatusAfter { get; set; }
    public DateTime? NextFollowupDate { get; set; }
    public string? NextFollowupNotes { get; set; }
    public int? FollowedByUserId { get; set; }

    // Navigation properties
    public Lead Lead { get; set; } = null!;
    public User? FollowedByUser { get; set; }
}

public enum FollowupType
{
    PhoneCall,
    Email,
    SMS,
    WhatsApp,
    Meeting,
    Visit,
    Other
}
