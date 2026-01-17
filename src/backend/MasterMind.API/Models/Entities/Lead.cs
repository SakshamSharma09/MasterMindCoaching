namespace MasterMind.API.Models.Entities;

/// <summary>
/// Lead entity for tracking inquiries and potential students
/// </summary>
public class Lead : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? ParentName { get; set; }
    public string? ParentMobile { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public LeadSource Source { get; set; }
    public string? SourceDetails { get; set; } // e.g., "Google Ads Campaign 1"
    public string? InterestedClass { get; set; }
    public string? InterestedSubject { get; set; }
    public LeadStatus Status { get; set; } = LeadStatus.New;
    public LeadPriority Priority { get; set; } = LeadPriority.Medium;
    public DateTime? NextFollowupDate { get; set; }
    public string? Remarks { get; set; }
    public int? AssignedToUserId { get; set; }
    public int? ConvertedStudentId { get; set; }
    public DateTime? ConvertedAt { get; set; }

    // Navigation properties
    public User? AssignedToUser { get; set; }
    public Student? ConvertedStudent { get; set; }
    public ICollection<LeadFollowup> LeadFollowups { get; set; } = new List<LeadFollowup>();
}

public enum LeadSource
{
    WalkIn,
    PhoneCall,
    Website,
    SocialMedia,
    Referral,
    Advertisement,
    Event,
    Other
}

public enum LeadStatus
{
    New,
    Contacted,
    Interested,
    FollowUp,
    Negotiation,
    Converted,
    Lost,
    NotInterested
}

public enum LeadPriority
{
    Low,
    Medium,
    High,
    Urgent
}
