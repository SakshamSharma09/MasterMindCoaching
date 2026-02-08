using MasterMind.API.Models.Entities;

namespace MasterMind.API.Services.Interfaces;

public interface IFinanceService
{
    // Financial Summary
    Task<FinancialSummary> GetFinancialSummaryAsync();
    
    // Payments
    Task<IEnumerable<Payment>> GetRecentPaymentsAsync(int limit = 10);
    Task<IEnumerable<Payment>> GetPaymentHistoryAsync(int? studentId = null, DateTime? startDate = null, DateTime? endDate = null);
    Task<Payment> CreatePaymentAsync(CreatePaymentDto paymentDto);
    Task<IEnumerable<Payment>> GetPendingPaymentsAsync();
    
    // Fees
    Task<IEnumerable<StudentFee>> GetFeesAsync(int? classId = null, string? status = null, string? month = null);
    Task<StudentFee> CreateFeeAsync(CreateFeeDto feeDto);
    Task<StudentFee> UpdateFeeAsync(int id, UpdateFeeDto feeDto);
    Task<bool> DeleteFeeAsync(int id);
    Task<IEnumerable<StudentFee>> GetOverdueFeesAsync();
    Task<bool> SendRemindersAsync(List<int>? feeIds = null);
    Task<StudentFee> MarkFeeAsPaidAsync(int id);
    
    // Fee Structures
    Task<IEnumerable<FeeStructure>> GetFeeStructuresAsync();
    
    // Expenses
    Task<IEnumerable<ExpenseDto>> GetExpensesAsync(string? category = null, DateTime? startDate = null, DateTime? endDate = null);
    Task<ExpenseDto> CreateExpenseAsync(CreateExpenseRequest expenseDto);
    Task<ExpenseDto> UpdateExpenseAsync(int id, UpdateExpenseRequest expenseDto);
    Task<bool> DeleteExpenseAsync(int id);
    Task<IEnumerable<string>> GetExpenseCategoriesAsync();
    Task<IEnumerable<ExpenseSummaryDto>> GetExpenseSummaryAsync(DateTime? startDate = null, DateTime? endDate = null);
    
    // Reports
    Task<FinancialReport> GenerateReportAsync(DateTime startDate, DateTime endDate);
}

// DTOs for Service Layer
public class CreatePaymentDto
{
    public int StudentFeeId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public string? TransactionId { get; set; }
    public string? InvoiceId { get; set; }
    public string? Description { get; set; }
}

public class CreateFeeDto
{
    public int StudentId { get; set; }
    public int FeeStructureId { get; set; }
    public decimal? DiscountAmount { get; set; }
    public string? DiscountReason { get; set; }
    public string DueDate { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Month { get; set; }
}

public class UpdateFeeDto
{
    public string? DueDate { get; set; }
    public decimal? DiscountAmount { get; set; }
    public string? Description { get; set; }
}

public class FinancialSummary
{
    public decimal TotalRevenue { get; set; }
    public decimal PendingPayments { get; set; }
    public decimal Expenses { get; set; }
    public decimal NetProfit { get; set; }
    public int TotalStudents { get; set; }
    public int PaidStudents { get; set; }
    public int PendingStudents { get; set; }
    public int OverdueStudents { get; set; }
}

public class PaymentDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Date { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? InvoiceId { get; set; }
}

public class StudentFeeDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string FeeType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string DueDate { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceAmount { get; set; }
    public bool IsOverdue { get; set; }
}

public class OverdueFeeDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string FeeType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string DueDate { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DaysOverdue { get; set; }
    public string ParentContact { get; set; } = string.Empty;
    public decimal BalanceAmount { get; set; }
}

public class ExpenseDto
{
    public long Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaidTo { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string? ReceiptNumber { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class CreateExpenseRequest
{
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaidTo { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string? ReceiptNumber { get; set; }
}

public class UpdateExpenseRequest
{
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaidTo { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string? ReceiptNumber { get; set; }
}

public class ExpenseSummaryDto
{
    public string Category { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int Count { get; set; }
}

public class FinancialReport
{
    public long ReportId { get; set; }
    public DateTime GeneratedAt { get; set; }
    public ReportPeriod Period { get; set; } = new();
    public ReportData Data { get; set; } = new();
}

public class ReportPeriod
{
    public string StartDate { get; set; } = string.Empty;
    public string EndDate { get; set; } = string.Empty;
}

public class ReportData
{
    public decimal TotalRevenue { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetProfit { get; set; }
    public List<PaymentDto> Payments { get; set; } = new();
    public List<ExpenseDto> Expenses { get; set; } = new();
}

public class FeeStructureDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Frequency { get; set; } = string.Empty;
    public int? ClassId { get; set; }
    public string? ClassName { get; set; }
    public string? Description { get; set; }
    public string AcademicYear { get; set; } = string.Empty;
}
