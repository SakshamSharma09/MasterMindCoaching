namespace MasterMind.API.Models.Entities;

/// <summary>
/// Student fee entity for tracking individual student fees
/// </summary>
public class StudentFee : BaseEntity
{
    public int StudentId { get; set; }
    public int FeeStructureId { get; set; }
    public decimal Amount { get; set; }
    public decimal? DiscountAmount { get; set; }
    public string? DiscountReason { get; set; }
    public decimal FinalAmount { get; set; }
    public decimal PaidAmount { get; set; } = 0;
    public DateOnly DueDate { get; set; }
    public FeeStatus Status { get; set; } = FeeStatus.Pending;
    public string? Remarks { get; set; }
    public string? Month { get; set; } // For monthly fees
    public string AcademicYear { get; set; } = string.Empty;

    // Navigation properties
    public Student Student { get; set; } = null!;
    public FeeStructure FeeStructure { get; set; } = null!;
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();

    // Computed properties
    public decimal BalanceAmount => FinalAmount - PaidAmount;
    public bool IsOverdue => Status != FeeStatus.Paid && DateOnly.FromDateTime(DateTime.Today) > DueDate;
}

public enum FeeStatus
{
    Pending,
    PartiallyPaid,
    Paid,
    Overdue,
    Waived,
    Cancelled
}
