namespace MasterMind.API.Models.Entities;

/// <summary>
/// Fee structure entity for defining fee types and amounts
/// </summary>
public class FeeStructure : BaseEntity
{
    public string Name { get; set; } = string.Empty; // e.g., "Monthly Tuition", "Admission Fee"
    public FeeType Type { get; set; }
    public decimal Amount { get; set; }
    public FeeFrequency Frequency { get; set; }
    public int? ClassId { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public string AcademicYear { get; set; } = string.Empty;

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

public enum FeeFrequency
{
    OneTime,
    Monthly,
    Quarterly,
    HalfYearly,
    Yearly
}
