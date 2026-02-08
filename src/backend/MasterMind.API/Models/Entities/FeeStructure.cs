namespace MasterMind.API.Models.Entities;

/// <summary>
/// Fee structure entity for defining fee types and amounts
/// </summary>
public class FeeStructure : BaseEntity
{
    public string Name { get; set; } = string.Empty; // e.g., "Monthly Tuition", "Full Course Fee"
    public FeeType Type { get; set; }
    public FeeCategory Category { get; set; } // NEW: Monthly vs Full Course
    public decimal Amount { get; set; }
    public FeeFrequency Frequency { get; set; }
    public int? ClassId { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public string AcademicYear { get; set; } = string.Empty;
    
    // NEW: Additional properties for comprehensive fee management
    public int DurationMonths { get; set; } // Duration for full course fees
    public decimal? LateFeePerDay { get; set; } // Late fee charges
    public decimal? DiscountAmount { get; set; } // Early payment discount
    public int? DiscountDaysBeforeDue { get; set; } // Discount validity period
    public bool IsRefundable { get; set; } = false;
    public decimal? RefundPercentage { get; set; }
    public string? TermsAndConditions { get; set; }

    // Navigation properties
    public Class? Class { get; set; }
    public ICollection<StudentFee> StudentFees { get; set; } = new List<StudentFee>();
}

public enum FeeType
{
    Tuition,
    Admission,
    Examination,
    Library,
    Laboratory,
    Sports,
    Transport,
    Uniform,
    Books,
    Other
}

// NEW: Fee category to distinguish Monthly vs Full Course
public enum FeeCategory
{
    Monthly,      // Monthly recurring fees
    FullCourse,   // One-time full course fees
    Additional    // Additional fees (materials, events, etc.)
}

public enum FeeFrequency
{
    OneTime,
    Monthly,
    Quarterly,
    HalfYearly,
    Yearly
}
