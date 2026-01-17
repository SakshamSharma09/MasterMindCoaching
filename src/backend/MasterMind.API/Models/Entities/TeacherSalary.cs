namespace MasterMind.API.Models.Entities;

/// <summary>
/// Teacher salary entity for tracking teacher salary payments
/// </summary>
public class TeacherSalary : BaseEntity
{
    public int TeacherId { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal? Allowances { get; set; }
    public decimal? Deductions { get; set; }
    public decimal NetSalary { get; set; }
    public string Month { get; set; } = string.Empty; // e.g., "January 2024"
    public int Year { get; set; }
    public DateTime? PaymentDate { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public string? TransactionId { get; set; }
    public SalaryStatus Status { get; set; } = SalaryStatus.Pending;
    public string? Remarks { get; set; }
    public int? ProcessedByUserId { get; set; }

    // Navigation properties
    public Teacher Teacher { get; set; } = null!;
    public User? ProcessedByUser { get; set; }
}

public enum SalaryStatus
{
    Pending,
    Processed,
    Paid,
    OnHold,
    Cancelled
}
