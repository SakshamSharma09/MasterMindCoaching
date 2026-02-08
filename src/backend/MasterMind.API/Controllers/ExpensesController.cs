using MasterMind.API.Data;
using MasterMind.API.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MasterMind.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ExpensesController : ControllerBase
{
    private readonly MasterMindDbContext _context;
    private readonly ILogger<ExpensesController> _logger;

    public ExpensesController(MasterMindDbContext context, ILogger<ExpensesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all expenses with optional filtering
    /// </summary>
    /// <param name="category">Optional category filter</param>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <returns>List of expenses</returns>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ExpenseDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ExpenseDto>>>> GetExpenses(
        [FromQuery] string? category,
        [FromQuery] string? startDate,
        [FromQuery] string? endDate)
    {
        try
        {
            var query = _context.Expenses
                .Include(e => e.ProcessedByUser)
                .Include(e => e.BudgetCategory)
                .AsQueryable();

            // Filter by date range if provided
            if (!string.IsNullOrEmpty(startDate) && DateTime.TryParse(startDate, out var start))
            {
                query = query.Where(e => e.ExpenseDate >= start);
            }

            if (!string.IsNullOrEmpty(endDate) && DateTime.TryParse(endDate, out var end))
            {
                query = query.Where(e => e.ExpenseDate <= end);
            }

            // Filter by category if provided
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(e => e.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
            }

            var expenses = await query
                .OrderByDescending(e => e.ExpenseDate)
                .Select(e => new ExpenseDto
                {
                    Id = e.Id,
                    Category = e.Category,
                    Description = e.Description,
                    Amount = e.Amount,
                    PaidTo = e.PaidTo,
                    Date = e.ExpenseDate.ToString("yyyy-MM-dd"),
                    ReceiptNumber = e.ReceiptNumber,
                    Status = e.Status.ToString(),
                    PaymentMethod = e.PaymentMethod.HasValue ? e.PaymentMethod.Value.ToString() : null,
                    VendorName = e.VendorName,
                    IsRecurring = e.IsRecurring,
                    ProcessedBy = e.ProcessedByUser != null ? $"{e.ProcessedByUser.FirstName} {e.ProcessedByUser.LastName}" : null
                })
                .ToListAsync();

            return Ok(new ApiResponse<IEnumerable<ExpenseDto>>
            {
                Success = true,
                Message = "Expenses retrieved successfully",
                Data = expenses
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving expenses");
            return StatusCode(500, new ApiResponse<IEnumerable<ExpenseDto>>
            {
                Success = false,
                Message = "Error retrieving expenses"
            });
        }
    }

    /// <summary>
    /// Create a new expense
    /// </summary>
    /// <param name="request">Expense creation request</param>
    /// <returns>Created expense</returns>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ExpenseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ExpenseDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ExpenseDto>>> CreateExpense([FromBody] CreateExpenseRequest request)
    {
        try
        {
            var expense = new Expense
            {
                Category = request.Category,
                Description = request.Description,
                Amount = request.Amount,
                PaidTo = request.PaidTo,
                ExpenseDate = DateTime.Parse(request.Date),
                ReceiptNumber = request.ReceiptNumber,
                InvoiceNumber = request.InvoiceNumber,
                Status = ExpenseStatus.Pending,
                PaymentMethod = request.PaymentMethod,
                TransactionId = request.TransactionId,
                Remarks = request.Remarks,
                VendorName = request.VendorName,
                VendorContact = request.VendorContact,
                IsRecurring = request.IsRecurring,
                RecurrencePattern = request.RecurrencePattern,
                ProcessedByUserId = GetCurrentUserId()
            };

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            var expenseDto = new ExpenseDto
            {
                Id = expense.Id,
                Category = expense.Category,
                Description = expense.Description,
                Amount = expense.Amount,
                PaidTo = expense.PaidTo,
                Date = expense.ExpenseDate.ToString("yyyy-MM-dd"),
                ReceiptNumber = expense.ReceiptNumber,
                Status = expense.Status.ToString(),
                PaymentMethod = expense.PaymentMethod?.ToString(),
                VendorName = expense.VendorName,
                IsRecurring = expense.IsRecurring,
                ProcessedBy = $"{User.Identity?.Name}"
            };

            _logger.LogInformation($"Expense created: {request.Category} - {request.Amount} paid to {request.PaidTo}");

            return CreatedAtAction(nameof(GetExpenses), new { }, new ApiResponse<ExpenseDto>
            {
                Success = true,
                Message = "Expense created successfully",
                Data = expenseDto
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating expense");
            return StatusCode(500, new ApiResponse<ExpenseDto>
            {
                Success = false,
                Message = "Error creating expense"
            });
        }
    }

    /// <summary>
    /// Update an existing expense
    /// </summary>
    /// <param name="id">Expense ID</param>
    /// <param name="request">Expense update request</param>
    /// <returns>Updated expense</returns>
    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ExpenseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ExpenseDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ExpenseDto>>> UpdateExpense(int id, [FromBody] UpdateExpenseRequest request)
    {
        try
        {
            var expense = await _context.Expenses
                .Include(e => e.ProcessedByUser)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (expense == null)
            {
                return NotFound(new ApiResponse<ExpenseDto>
                {
                    Success = false,
                    Message = "Expense not found"
                });
            }

            // Update expense properties
            expense.Category = request.Category;
            expense.Description = request.Description;
            expense.Amount = request.Amount;
            expense.PaidTo = request.PaidTo;
            expense.ExpenseDate = DateTime.Parse(request.Date);
            expense.ReceiptNumber = request.ReceiptNumber;
            expense.InvoiceNumber = request.InvoiceNumber;
            expense.PaymentMethod = request.PaymentMethod;
            expense.TransactionId = request.TransactionId;
            expense.Remarks = request.Remarks;
            expense.VendorName = request.VendorName;
            expense.VendorContact = request.VendorContact;
            expense.IsRecurring = request.IsRecurring;
            expense.RecurrencePattern = request.RecurrencePattern;

            await _context.SaveChangesAsync();

            var expenseDto = new ExpenseDto
            {
                Id = expense.Id,
                Category = expense.Category,
                Description = expense.Description,
                Amount = expense.Amount,
                PaidTo = expense.PaidTo,
                Date = expense.ExpenseDate.ToString("yyyy-MM-dd"),
                ReceiptNumber = expense.ReceiptNumber,
                Status = expense.Status.ToString(),
                PaymentMethod = expense.PaymentMethod?.ToString(),
                VendorName = expense.VendorName,
                IsRecurring = expense.IsRecurring,
                ProcessedBy = expense.ProcessedByUser != null ? $"{expense.ProcessedByUser.FirstName} {expense.ProcessedByUser.LastName}" : null
            };

            _logger.LogInformation($"Expense {id} updated: {request.Category} - {request.Amount}");

            return Ok(new ApiResponse<ExpenseDto>
            {
                Success = true,
                Message = "Expense updated successfully",
                Data = expenseDto
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating expense");
            return StatusCode(500, new ApiResponse<ExpenseDto>
            {
                Success = false,
                Message = "Error updating expense"
            });
        }
    }

    /// <summary>
    /// Delete an expense
    /// </summary>
    /// <param name="id">Expense ID</param>
    /// <returns>Success response</returns>
    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteExpense(int id)
    {
        try
        {
            var expense = await _context.Expenses.FindAsync(id);

            if (expense == null)
            {
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Expense not found"
                });
            }

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Expense {id} deleted");

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = "Expense deleted successfully",
                Data = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting expense");
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = "Error deleting expense"
            });
        }
    }

    /// <summary>
    /// Get expense categories
    /// </summary>
    /// <returns>List of expense categories</returns>
    [HttpGet("categories")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<string>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<string>>>> GetExpenseCategories()
    {
        try
        {
            var categories = new List<string>
            {
                "Salary",
                "Rent",
                "Utilities",
                "Supplies",
                "Maintenance",
                "Marketing",
                "Other"
            };

            return Ok(new ApiResponse<IEnumerable<string>>
            {
                Success = true,
                Message = "Expense categories retrieved successfully",
                Data = categories
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving expense categories");
            return StatusCode(500, new ApiResponse<IEnumerable<string>>
            {
                Success = false,
                Message = "Error retrieving expense categories"
            });
        }
    }

    /// <summary>
    /// Get expense summary by category
    /// </summary>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <returns>Expense summary by category</returns>
    [HttpGet("summary")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ExpenseSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ExpenseSummaryDto>>>> GetExpenseSummary(
        [FromQuery] string? startDate,
        [FromQuery] string? endDate)
    {
        try
        {
            var query = _context.Expenses.AsQueryable();

            // Filter by date range if provided
            if (!string.IsNullOrEmpty(startDate) && DateTime.TryParse(startDate, out var start))
            {
                query = query.Where(e => e.ExpenseDate >= start);
            }

            if (!string.IsNullOrEmpty(endDate) && DateTime.TryParse(endDate, out var end))
            {
                query = query.Where(e => e.ExpenseDate <= end);
            }

            var expenses = await query
                .Where(e => e.Status == ExpenseStatus.Paid || e.Status == ExpenseStatus.Processed)
                .ToListAsync();

            var summary = expenses
                .GroupBy(e => e.Category)
                .Select(g => new ExpenseSummaryDto
                {
                    Category = g.Key,
                    TotalAmount = g.Sum(e => e.Amount),
                    Count = g.Count()
                })
                .OrderByDescending(s => s.TotalAmount)
                .ToList();

            return Ok(new ApiResponse<IEnumerable<ExpenseSummaryDto>>
            {
                Success = true,
                Message = "Expense summary retrieved successfully",
                Data = summary
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving expense summary");
            return StatusCode(500, new ApiResponse<IEnumerable<ExpenseSummaryDto>>
            {
                Success = false,
                Message = "Error retrieving expense summary"
            });
        }
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}

// DTOs
public class ExpenseDto
{
    public int Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaidTo { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string? ReceiptNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? PaymentMethod { get; set; }
    public string? VendorName { get; set; }
    public bool IsRecurring { get; set; }
    public string? ProcessedBy { get; set; }
}

public class CreateExpenseRequest
{
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaidTo { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string? ReceiptNumber { get; set; }
    public string? InvoiceNumber { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public string? TransactionId { get; set; }
    public string? Remarks { get; set; }
    public string? VendorName { get; set; }
    public string? VendorContact { get; set; }
    public bool IsRecurring { get; set; }
    public string? RecurrencePattern { get; set; }
}

public class UpdateExpenseRequest
{
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaidTo { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string? ReceiptNumber { get; set; }
    public string? InvoiceNumber { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public string? TransactionId { get; set; }
    public string? Remarks { get; set; }
    public string? VendorName { get; set; }
    public string? VendorContact { get; set; }
    public bool IsRecurring { get; set; }
    public string? RecurrencePattern { get; set; }
}

public class ExpenseSummaryDto
{
    public string Category { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int Count { get; set; }
}
