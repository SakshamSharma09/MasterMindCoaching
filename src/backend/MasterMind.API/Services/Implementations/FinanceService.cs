using MasterMind.API.Data;
using MasterMind.API.Models.Entities;
using MasterMind.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MasterMind.API.Services.Implementations;

public class FinanceService : IFinanceService
{
    private readonly MasterMindDbContext _context;
    private readonly ILogger<FinanceService> _logger;

    public FinanceService(MasterMindDbContext context, ILogger<FinanceService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<FinancialSummary> GetFinancialSummaryAsync()
    {
        var currentSession = await _context.Sessions
            .Where(s => s.IsActive)
            .FirstOrDefaultAsync();

        var totalStudents = await _context.Students
            .CountAsync(s => !s.IsDeleted && s.IsActive && (currentSession == null || s.SessionId == currentSession.Id));

        // Calculate total revenue from payments
        var totalRevenue = await _context.Payments
            .Where(p => p.Status == PaymentStatus.Completed)
            .SumAsync(p => (decimal)p.Amount);

        // Calculate pending payments
        var pendingPayments = await _context.StudentFees
            .Where(sf => sf.Status == FeeStatus.Pending || sf.Status == FeeStatus.PartiallyPaid)
            .SumAsync(sf => sf.BalanceAmount);

        // Calculate total expenses (teacher salaries + other expenses)
        var totalSalaries = await _context.TeacherSalaries
            .Where(ts => ts.Status == SalaryStatus.Paid)
            .SumAsync(ts => ts.NetSalary);

        var totalExpenses = totalSalaries;
        var netProfit = totalRevenue - totalExpenses;

        var paidStudents = await _context.StudentFees
            .Where(sf => sf.Status == FeeStatus.Paid)
            .Select(sf => sf.StudentId)
            .Distinct()
            .CountAsync();

        var pendingStudents = await _context.StudentFees
            .Where(sf => sf.Status == FeeStatus.Pending || sf.Status == FeeStatus.PartiallyPaid)
            .Select(sf => sf.StudentId)
            .Distinct()
            .CountAsync();

        var overdueStudents = await _context.StudentFees
            .Where(sf => sf.Status != FeeStatus.Paid && DateOnly.FromDateTime(DateTime.Today) > sf.DueDate)
            .Select(sf => sf.StudentId)
            .Distinct()
            .CountAsync();

        return new FinancialSummary
        {
            TotalRevenue = totalRevenue,
            PendingPayments = pendingPayments,
            Expenses = totalExpenses,
            NetProfit = netProfit,
            TotalStudents = totalStudents,
            PaidStudents = paidStudents,
            PendingStudents = pendingStudents,
            OverdueStudents = overdueStudents
        };
    }

    public async Task<IEnumerable<Payment>> GetRecentPaymentsAsync(int limit = 10)
    {
        return await _context.Payments
            .Include(p => p.StudentFee)
                .ThenInclude(sf => sf.Student)
            .Include(p => p.StudentFee.FeeStructure)
            .Where(p => p.Status == PaymentStatus.Completed)
            .OrderByDescending(p => p.PaymentDate)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<Payment>> GetPaymentHistoryAsync(int? studentId = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.Payments
            .Include(p => p.StudentFee)
                .ThenInclude(sf => sf.Student)
            .Include(p => p.StudentFee.FeeStructure)
            .AsQueryable();

        if (studentId.HasValue)
        {
            query = query.Where(p => p.StudentFee.StudentId == studentId.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(p => p.PaymentDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(p => p.PaymentDate <= endDate.Value);
        }

        return await query
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();
    }

    public async Task<Payment> CreatePaymentAsync(CreatePaymentDto paymentDto)
    {
        var studentFee = await _context.StudentFees
            .Include(sf => sf.Student)
            .Include(sf => sf.FeeStructure)
            .FirstOrDefaultAsync(sf => sf.Id == paymentDto.StudentFeeId);

        if (studentFee == null)
        {
            throw new ArgumentException("Student fee not found", nameof(paymentDto.StudentFeeId));
        }

        if (paymentDto.Amount > studentFee.BalanceAmount)
        {
            throw new ArgumentException("Payment amount exceeds balance amount", nameof(paymentDto.Amount));
        }

        var payment = new Payment
        {
            StudentFeeId = paymentDto.StudentFeeId,
            Amount = paymentDto.Amount,
            Method = paymentDto.Method,
            TransactionId = paymentDto.TransactionId,
            ReceiptNumber = paymentDto.InvoiceId,
            Remarks = paymentDto.Description,
            Status = PaymentStatus.Completed
        };

        _context.Payments.Add(payment);

        // Update student fee paid amount and status
        studentFee.PaidAmount += paymentDto.Amount;
        if (studentFee.PaidAmount >= studentFee.FinalAmount)
        {
            studentFee.Status = FeeStatus.Paid;
            studentFee.PaidAmount = studentFee.FinalAmount;
        }
        else
        {
            studentFee.Status = FeeStatus.PartiallyPaid;
        }

        await _context.SaveChangesAsync();
        return payment;
    }

    public async Task<IEnumerable<Payment>> GetPendingPaymentsAsync()
    {
        var pendingStudentFeeIds = await _context.StudentFees
            .Where(sf => sf.Status == FeeStatus.Pending || sf.Status == FeeStatus.PartiallyPaid)
            .Select(sf => sf.Id)
            .ToListAsync();

        return await _context.Payments
            .Include(p => p.StudentFee)
                .ThenInclude(sf => sf.Student)
            .Include(p => p.StudentFee.FeeStructure)
            .Where(p => pendingStudentFeeIds.Contains(p.StudentFeeId))
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<StudentFee>> GetFeesAsync(int? classId = null, string? status = null, string? month = null)
    {
        var query = _context.StudentFees
            .Include(sf => sf.Student)
                .ThenInclude(s => s.StudentClasses)
                    .ThenInclude(sc => sc.Class)
            .Include(sf => sf.FeeStructure)
            .AsQueryable();

        if (classId.HasValue)
        {
            query = query.Where(sf => sf.Student.StudentClasses.Any(sc => sc.ClassId == classId.Value));
        }

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<FeeStatus>(status, true, out var feeStatus))
        {
            query = query.Where(sf => sf.Status == feeStatus);
        }

        if (!string.IsNullOrEmpty(month))
        {
            query = query.Where(sf => sf.Month == month);
        }

        return await query
            .OrderByDescending(sf => sf.DueDate)
            .ToListAsync();
    }

    public async Task<StudentFee> CreateFeeAsync(CreateFeeDto feeDto)
    {
        var student = await _context.Students
            .Include(s => s.StudentClasses)
                .ThenInclude(sc => sc.Class)
            .FirstOrDefaultAsync(s => s.Id == feeDto.StudentId);

        if (student == null)
        {
            throw new ArgumentException("Student not found", nameof(feeDto.StudentId));
        }

        var feeStructure = await _context.FeeStructures
            .FirstOrDefaultAsync(fs => fs.Id == feeDto.FeeStructureId);

        if (feeStructure == null)
        {
            throw new ArgumentException("Fee structure not found", nameof(feeDto.FeeStructureId));
        }

        var studentFee = new StudentFee
        {
            StudentId = feeDto.StudentId,
            FeeStructureId = feeDto.FeeStructureId,
            Amount = feeStructure.Amount,
            DiscountAmount = feeDto.DiscountAmount,
            DiscountReason = feeDto.DiscountReason,
            FinalAmount = feeStructure.Amount - (feeDto.DiscountAmount ?? 0),
            DueDate = DateOnly.Parse(feeDto.DueDate),
            Status = FeeStatus.Pending,
            Remarks = feeDto.Description,
            Month = feeDto.Month,
            AcademicYear = GetCurrentAcademicYear()
        };

        _context.StudentFees.Add(studentFee);
        await _context.SaveChangesAsync();

        return studentFee;
    }

    public async Task<StudentFee> UpdateFeeAsync(int id, UpdateFeeDto feeDto)
    {
        var studentFee = await _context.StudentFees
            .Include(sf => sf.Student)
                .ThenInclude(s => s.StudentClasses)
                    .ThenInclude(sc => sc.Class)
            .Include(sf => sf.FeeStructure)
            .FirstOrDefaultAsync(sf => sf.Id == id);

        if (studentFee == null)
        {
            throw new ArgumentException("Fee not found", nameof(id));
        }

        // Don't allow updates if fee is already paid
        if (studentFee.Status == FeeStatus.Paid)
        {
            throw new InvalidOperationException("Cannot update a paid fee");
        }

        if (!string.IsNullOrEmpty(feeDto.DueDate))
        {
            studentFee.DueDate = DateOnly.Parse(feeDto.DueDate);
        }

        if (!string.IsNullOrEmpty(feeDto.Description))
        {
            studentFee.Remarks = feeDto.Description;
        }

        if (feeDto.DiscountAmount.HasValue)
        {
            studentFee.DiscountAmount = feeDto.DiscountAmount.Value;
            studentFee.FinalAmount = studentFee.Amount - feeDto.DiscountAmount.Value;
            // BalanceAmount is now computed automatically: FinalAmount - PaidAmount
        }

        await _context.SaveChangesAsync();
        return studentFee;
    }

    public async Task<bool> DeleteFeeAsync(int id)
    {
        var studentFee = await _context.StudentFees
            .Include(sf => sf.Payments)
            .FirstOrDefaultAsync(sf => sf.Id == id);

        if (studentFee == null)
        {
            return false;
        }

        // Don't allow deletion if there are payments
        if (studentFee.Payments.Any())
        {
            throw new InvalidOperationException("Cannot delete fee with existing payments");
        }

        _context.StudentFees.Remove(studentFee);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<StudentFee>> GetOverdueFeesAsync()
    {
        return await _context.StudentFees
            .Include(sf => sf.Student)
                .ThenInclude(s => s.StudentClasses)
                    .ThenInclude(sc => sc.Class)
            .Include(sf => sf.FeeStructure)
            .Where(sf => sf.Status != FeeStatus.Paid && DateOnly.FromDateTime(DateTime.Today) > sf.DueDate)
            .OrderBy(sf => sf.DueDate)
            .ToListAsync();
    }

    public async Task<bool> SendRemindersAsync(List<int>? feeIds = null)
    {
        var query = _context.StudentFees
            .Include(sf => sf.Student)
            .Where(sf => sf.Status != FeeStatus.Paid && DateOnly.FromDateTime(DateTime.Today) > sf.DueDate);

        if (feeIds != null && feeIds.Any())
        {
            query = query.Where(sf => feeIds.Contains(sf.Id));
        }

        var overdueFees = await query.ToListAsync();

        // Here you would implement actual SMS/Email sending logic
        foreach (var fee in overdueFees)
        {
            _logger.LogInformation($"Reminder sent to {fee.Student.ParentName} ({fee.Student.ParentMobile}) for fee {fee.Id}");
        }

        return true;
    }

    public async Task<StudentFee> MarkFeeAsPaidAsync(int id)
    {
        var studentFee = await _context.StudentFees
            .Include(sf => sf.Student)
                .ThenInclude(s => s.StudentClasses)
                    .ThenInclude(sc => sc.Class)
            .Include(sf => sf.FeeStructure)
            .FirstOrDefaultAsync(sf => sf.Id == id);

        if (studentFee == null)
        {
            throw new ArgumentException("Fee not found", nameof(id));
        }

        // Create a payment for the remaining balance
        if (studentFee.BalanceAmount > 0)
        {
            var payment = new Payment
            {
                StudentFeeId = studentFee.Id,
                Amount = studentFee.BalanceAmount,
                Method = PaymentMethod.Cash, // Default method
                Status = PaymentStatus.Completed,
                Remarks = "Marked as paid manually"
            };

            _context.Payments.Add(payment);
        }

        studentFee.Status = FeeStatus.Paid;
        studentFee.PaidAmount = studentFee.FinalAmount;

        await _context.SaveChangesAsync();
        return studentFee;
    }

    public async Task<IEnumerable<FeeStructure>> GetFeeStructuresAsync()
    {
        return await _context.FeeStructures
            .Include(fs => fs.Class)
            .Where(fs => fs.IsActive)
            .OrderBy(fs => fs.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<ExpenseDto>> GetExpensesAsync(string? category = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.TeacherSalaries
            .Include(ts => ts.Teacher)
            .AsQueryable();

        // Filter by date range if provided
        if (startDate.HasValue)
        {
            query = query.Where(ts => ts.PaymentDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(ts => ts.PaymentDate <= endDate.Value);
        }

        var salaries = await query
            .OrderByDescending(ts => ts.PaymentDate)
            .ToListAsync();

        var expenses = new List<ExpenseDto>();

        foreach (var salary in salaries)
        {
            expenses.Add(new ExpenseDto
            {
                Id = salary.Id,
                Category = "Salary",
                Description = $"Salary for {salary.Teacher.FirstName} {salary.Teacher.LastName} - {salary.Month}",
                Amount = salary.NetSalary,
                PaidTo = $"{salary.Teacher.FirstName} {salary.Teacher.LastName}",
                Date = salary.PaymentDate?.ToString("yyyy-MM-dd") ?? salary.CreatedAt.ToString("yyyy-MM-dd"),
                ReceiptNumber = salary.TransactionId,
                Status = salary.Status.ToString()
            });
        }

        // Filter by category if provided
        if (!string.IsNullOrEmpty(category))
        {
            expenses = expenses.Where(e => e.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return expenses;
    }

    public async Task<ExpenseDto> CreateExpenseAsync(CreateExpenseRequest expenseDto)
    {
        // For now, we'll create a mock expense since we don't have a dedicated Expense table
        var expense = new ExpenseDto
        {
            Id = DateTime.UtcNow.Ticks,
            Category = expenseDto.Category,
            Description = expenseDto.Description,
            Amount = expenseDto.Amount,
            PaidTo = expenseDto.PaidTo,
            Date = expenseDto.Date,
            ReceiptNumber = expenseDto.ReceiptNumber,
            Status = "Completed"
        };

        _logger.LogInformation($"Expense created: {expenseDto.Category} - {expenseDto.Amount} paid to {expenseDto.PaidTo}");

        return expense;
    }

    public async Task<ExpenseDto> UpdateExpenseAsync(int id, UpdateExpenseRequest expenseDto)
    {
        // For now, we'll simulate the update since we don't have a dedicated Expense table
        _logger.LogInformation($"Expense {id} updated: {expenseDto.Category} - {expenseDto.Amount}");

        var expense = new ExpenseDto
        {
            Id = id,
            Category = expenseDto.Category,
            Description = expenseDto.Description,
            Amount = expenseDto.Amount,
            PaidTo = expenseDto.PaidTo,
            Date = expenseDto.Date,
            ReceiptNumber = expenseDto.ReceiptNumber,
            Status = "Completed"
        };

        return expense;
    }

    public async Task<bool> DeleteExpenseAsync(int id)
    {
        // For now, we'll simulate the deletion since we don't have a dedicated Expense table
        _logger.LogInformation($"Expense {id} deleted");
        return true;
    }

    public async Task<IEnumerable<string>> GetExpenseCategoriesAsync()
    {
        return new List<string>
        {
            "Salary",
            "Rent",
            "Utilities",
            "Supplies",
            "Maintenance",
            "Marketing",
            "Other"
        };
    }

    public async Task<IEnumerable<ExpenseSummaryDto>> GetExpenseSummaryAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.TeacherSalaries.AsQueryable();

        // Filter by date range if provided
        if (startDate.HasValue)
        {
            query = query.Where(ts => ts.PaymentDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(ts => ts.PaymentDate <= endDate.Value);
        }

        var salaries = await query
            .Where(ts => ts.Status == SalaryStatus.Paid)
            .ToListAsync();

        var summary = new List<ExpenseSummaryDto>
        {
            new ExpenseSummaryDto
            {
                Category = "Salary",
                TotalAmount = salaries.Sum(s => s.NetSalary),
                Count = salaries.Count
            }
        };

        // Add other categories with zero amounts for now
        summary.Add(new ExpenseSummaryDto { Category = "Rent", TotalAmount = 0, Count = 0 });
        summary.Add(new ExpenseSummaryDto { Category = "Utilities", TotalAmount = 0, Count = 0 });
        summary.Add(new ExpenseSummaryDto { Category = "Supplies", TotalAmount = 0, Count = 0 });
        summary.Add(new ExpenseSummaryDto { Category = "Maintenance", TotalAmount = 0, Count = 0 });
        summary.Add(new ExpenseSummaryDto { Category = "Marketing", TotalAmount = 0, Count = 0 });
        summary.Add(new ExpenseSummaryDto { Category = "Other", TotalAmount = 0, Count = 0 });

        return summary;
    }

    public async Task<FinancialReport> GenerateReportAsync(DateTime startDate, DateTime endDate)
    {
        var payments = await _context.Payments
            .Include(p => p.StudentFee)
                .ThenInclude(sf => sf.Student)
            .Where(p => p.Status == PaymentStatus.Completed &&
                       p.PaymentDate >= startDate &&
                       p.PaymentDate <= endDate)
            .ToListAsync();

        var totalRevenue = payments.Sum(p => p.Amount);

        var expenses = await _context.TeacherSalaries
            .Where(ts => ts.Status == SalaryStatus.Paid &&
                       ts.PaymentDate >= startDate &&
                       ts.PaymentDate <= endDate)
            .ToListAsync();

        var totalExpenses = expenses.Sum(e => e.NetSalary);

        var report = new FinancialReport
        {
            ReportId = DateTime.UtcNow.Ticks,
            GeneratedAt = DateTime.UtcNow,
            Period = new ReportPeriod
            {
                StartDate = startDate.ToString("yyyy-MM-dd"),
                EndDate = endDate.ToString("yyyy-MM-dd")
            },
            Data = new ReportData
            {
                TotalRevenue = totalRevenue,
                TotalExpenses = totalExpenses,
                NetProfit = totalRevenue - totalExpenses,
                Payments = payments.Select(p => new PaymentDto
                {
                    Id = p.Id,
                    StudentId = p.StudentFee.StudentId,
                    StudentName = $"{p.StudentFee.Student.FirstName} {p.StudentFee.Student.LastName}",
                    Amount = p.Amount,
                    Date = p.PaymentDate.ToString("yyyy-MM-dd"),
                    Status = p.Status.ToString(),
                    Method = p.Method.ToString(),
                    Description = p.StudentFee.FeeStructure.Name,
                    InvoiceId = p.ReceiptNumber
                }).ToList(),
                Expenses = expenses.Select(e => new ExpenseDto
                {
                    Id = e.Id,
                    Category = "Salary",
                    Description = $"Salary for {e.Teacher.FirstName} {e.Teacher.LastName}",
                    Amount = e.NetSalary,
                    PaidTo = $"{e.Teacher.FirstName} {e.Teacher.LastName}",
                    Date = e.PaymentDate?.ToString("yyyy-MM-dd") ?? "",
                    ReceiptNumber = e.TransactionId,
                    Status = e.Status.ToString()
                }).ToList()
            }
        };

        return report;
    }

    private string GetCurrentAcademicYear()
    {
        var currentYear = DateTime.Now.Year;
        var nextYear = currentYear + 1;
        return $"{currentYear}-{nextYear}";
    }
}
