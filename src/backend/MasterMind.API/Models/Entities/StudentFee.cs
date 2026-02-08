using System.ComponentModel.DataAnnotations.Schema;

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
    public string? Month { get; set; } // For monthly fees (e.g., "2024-01")
    public string AcademicYear { get; set; } = string.Empty;
    
    // Enhanced properties for fee type management
    public FeeCategory FeeCategory { get; set; } // Monthly, FullCourse, Additional
    public DateOnly? StartDate { get; set; } // When the fee becomes active
    public DateOnly? EndDate { get; set; } // When the fee expires (for course fees)
    public int? RecurringDayOfMonth { get; set; } // Day of month for monthly fees (1-31)
    public bool IsRecurring { get; set; } = false; // Whether this fee generates recurring instances
    public int? ParentFeeId { get; set; } // For linking recurring fees to main fee
    public decimal? LateFeePerDay { get; set; } // Late fee charges
    public int GracePeriodDays { get; set; } = 0; // Grace period before late fees apply

    // Navigation properties
    public Student Student { get; set; } = null!;
    public FeeStructure FeeStructure { get; set; } = null!;
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<StudentFee> RecurringFees { get; set; } = new List<StudentFee>(); // Child recurring fees
    public StudentFee? ParentFee { get; set; } // Parent fee for recurring instances

    // Computed properties (not mapped to database)
    [NotMapped]
    public decimal BalanceAmount => FinalAmount - PaidAmount;
    
    [NotMapped]
    public bool IsOverdue 
    {
        get
        {
            if (Status == FeeStatus.Paid || Status == FeeStatus.Waived || Status == FeeStatus.Cancelled)
                return false;
                
            var today = DateOnly.FromDateTime(DateTime.Today);
            
            // Different overdue logic based on fee category
            return FeeCategory switch
            {
                FeeCategory.Monthly => today > DueDate,
                FeeCategory.FullCourse => StartDate.HasValue && today > StartDate.Value,
                FeeCategory.Additional => today > DueDate,
                _ => today > DueDate
            };
        }
    }
    
    [NotMapped]
    public int DaysOverdue
    {
        get
        {
            if (!IsOverdue) return 0;
            
            var today = DateOnly.FromDateTime(DateTime.Today);
            var overdueDate = FeeCategory switch
            {
                FeeCategory.FullCourse when StartDate.HasValue => StartDate.Value,
                _ => DueDate
            };
            
            return today.DayNumber - overdueDate.DayNumber;
        }
    }
    
    [NotMapped]
    public decimal LateFeeAmount
    {
        get
        {
            if (!IsOverdue || !LateFeePerDay.HasValue || DaysOverdue <= GracePeriodDays)
                return 0;
                
            var lateDays = DaysOverdue - GracePeriodDays;
            return lateDays * LateFeePerDay.Value;
        }
    }
    
    [NotMapped]
    public decimal TotalAmountDue => FinalAmount + LateFeeAmount;
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
