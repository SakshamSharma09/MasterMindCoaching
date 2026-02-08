namespace MasterMind.API.Models.Entities;

/// <summary>
/// Fee receipt entity for generating and managing fee receipts
/// </summary>
public class FeeReceipt : BaseEntity
{
    public string ReceiptNumber { get; set; } = string.Empty; // Unique receipt number
    public int StudentId { get; set; }
    public int PaymentId { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceAmount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public DateTime ReceiptDate { get; set; } = DateTime.UtcNow;
    public string ReceiptStatus { get; set; } = "Generated"; // Generated, Sent, Printed, Cancelled
    
    // Student and payment details at time of receipt
    public string StudentName { get; set; } = string.Empty;
    public string StudentClass { get; set; } = string.Empty;
    public string FeeDescription { get; set; } = string.Empty;
    public string FeePeriod { get; set; } = string.Empty; // e.g., "January 2024", "Full Course 2024-25"
    
    // Contact and communication
    public string ParentName { get; set; } = string.Empty;
    public string ParentEmail { get; set; } = string.Empty;
    public string ParentMobile { get; set; } = string.Empty;
    public bool IsEmailSent { get; set; } = false;
    public DateTime? EmailSentAt { get; set; }
    public bool IsSmsSent { get; set; } = false;
    public DateTime? SmsSentAt { get; set; }
    
    // Additional details
    public string? Remarks { get; set; }
    public string? PaymentNotes { get; set; }
    public int? GeneratedByUserId { get; set; }
    public string? InstitutionName { get; set; }
    public string? InstitutionAddress { get; set; }
    public string? InstitutionContact { get; set; }
    public string? InstitutionLogo { get; set; }

    // Navigation properties
    public Student Student { get; set; } = null!;
    public Payment Payment { get; set; } = null!;
    public User? GeneratedByUser { get; set; }
    public ICollection<FeeReceiptItem> ReceiptItems { get; set; } = new List<FeeReceiptItem>();
}

/// <summary>
/// Individual fee items within a receipt (for detailed breakdown)
/// </summary>
public class FeeReceiptItem : BaseEntity
{
    public int FeeReceiptId { get; set; }
    public string ItemDescription { get; set; } = string.Empty; // e.g., "Monthly Tuition - January"
    public decimal ItemAmount { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public string? Period { get; set; } // e.g., "January 2024"
    public int? StudentFeeId { get; set; }

    // Navigation properties
    public FeeReceipt FeeReceipt { get; set; } = null!;
    public StudentFee? StudentFee { get; set; }
}

/// <summary>
/// Fee payment schedule for recurring monthly fees
/// </summary>
public class FeePaymentSchedule : BaseEntity
{
    public int StudentId { get; set; }
    public int FeeStructureId { get; set; }
    public string ScheduleType { get; set; } = string.Empty; // "Monthly", "FullCourse"
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal MonthlyAmount { get; set; }
    public int TotalInstallments { get; set; }
    public int PaidInstallments { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public string? AcademicYear { get; set; }
    
    // Navigation properties
    public Student Student { get; set; } = null!;
    public FeeStructure FeeStructure { get; set; } = null!;
    public ICollection<FeeInstallment> Installments { get; set; } = new List<FeeInstallment>();
}

/// <summary>
/// Individual installments for payment schedules
/// </summary>
public class FeeInstallment : BaseEntity
{
    public int FeePaymentScheduleId { get; set; }
    public int InstallmentNumber { get; set; }
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public decimal? PaidAmount { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, PartiallyPaid, Paid, Overdue
    public decimal? LateFee { get; set; }
    public decimal? DiscountApplied { get; set; }
    public string? Remarks { get; set; }
    
    // Navigation properties
    public FeePaymentSchedule FeePaymentSchedule { get; set; } = null!;
    public StudentFee? StudentFee { get; set; }
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
