namespace MasterMind.API.Models.Entities;

/// <summary>
/// Expense entity for tracking institutional expenses
/// </summary>
public class Expense : BaseEntity
{
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaidTo { get; set; } = string.Empty;
    public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;
    public string? ReceiptNumber { get; set; }
    public string? InvoiceNumber { get; set; }
    public ExpenseStatus Status { get; set; } = ExpenseStatus.Pending;
    public PaymentMethod? PaymentMethod { get; set; }
    public string? TransactionId { get; set; }
    public string? Remarks { get; set; }
    public int? ProcessedByUserId { get; set; }
    public string? VendorName { get; set; }
    public string? VendorContact { get; set; }
    public bool IsRecurring { get; set; } = false;
    public string? RecurrencePattern { get; set; } // e.g., "Monthly", "Quarterly", "Yearly"
    public DateTime? NextDueDate { get; set; }
    public int? BudgetCategoryId { get; set; }

    // Navigation properties
    public User? ProcessedByUser { get; set; }
    public BudgetCategory? BudgetCategory { get; set; }
}

public enum ExpenseStatus
{
    Pending,
    Approved,
    Processed,
    Paid,
    Rejected,
    Cancelled
}

public enum ExpenseCategory
{
    Salary,
    Rent,
    Utilities,
    Supplies,
    Maintenance,
    Marketing,
    Technology,
    Insurance,
    Taxes,
    Legal,
    Training,
    Events,
    Transportation,
    Equipment,
    Furniture,
    Books,
    Software,
    Subscriptions,
    Other
}

/// <summary>
/// Budget category for expense tracking and budgeting
/// </summary>
public class BudgetCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal AllocatedBudget { get; set; }
    public decimal SpentAmount { get; set; } = 0;
    public string FiscalYear { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int? ParentCategoryId { get; set; }

    // Navigation properties
    public BudgetCategory? ParentCategory { get; set; }
    public ICollection<BudgetCategory> SubCategories { get; set; } = new List<BudgetCategory>();
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}
