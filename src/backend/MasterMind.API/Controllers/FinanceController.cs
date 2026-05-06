using MasterMind.API.Data;
using MasterMind.API.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace MasterMind.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class FinanceController : ControllerBase
{
    private readonly MasterMindDbContext _context;
    private readonly ILogger<FinanceController> _logger;
    private static readonly Dictionary<int, (string Name, FeeType Type)> LegacyFeeStructureCatalog = new()
    {
        [1] = ("Tuition Fee", FeeType.Tuition),
        [2] = ("Exam Fee", FeeType.Examination),
        [3] = ("Lab Fee", FeeType.Laboratory),
        [4] = ("Library Fee", FeeType.Library),
        [5] = ("Sports Fee", FeeType.Sports),
        [6] = ("Transport Fee", FeeType.Transport),
        [7] = ("Other Fee", FeeType.Other)
    };

    public FinanceController(MasterMindDbContext context, ILogger<FinanceController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get financial summary
    /// </summary>
    /// <returns>Financial summary data</returns>
    [HttpGet("summary")]
    //[Authorize]
    [ProducesResponseType(typeof(ApiResponse<FinancialSummary>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FinancialSummary>>> GetFinancialSummary([FromQuery] int? sessionId = null)
    {
        try
        {
            var activeSessionId = sessionId ?? await _context.Sessions
                .Where(s => s.IsActive && !s.IsDeleted)
                .Select(s => (int?)s.Id)
                .FirstOrDefaultAsync();

            var totalStudents = await _context.Students
                .CountAsync(s => !s.IsDeleted && s.IsActive &&
                    (!activeSessionId.HasValue || s.SessionId == activeSessionId));

            // Calculate total revenue from payments
            var totalRevenue = await _context.Payments
                .Where(p => p.Status == PaymentStatus.Completed &&
                    (!activeSessionId.HasValue || p.StudentFee.Student.SessionId == activeSessionId))
                .SumAsync(p => (decimal)p.Amount);

            // Calculate pending payments
            var pendingPayments = await _context.StudentFees
                .Where(sf => (sf.Status == FeeStatus.Pending || sf.Status == FeeStatus.PartiallyPaid) &&
                    (!activeSessionId.HasValue || sf.Student.SessionId == activeSessionId))
                .SumAsync(sf => sf.FinalAmount - sf.PaidAmount);

            // Calculate total expenses (teacher salaries + other expenses)
            var totalSalaries = await _context.TeacherSalaries
                .Where(ts => ts.Status == SalaryStatus.Paid)
                .SumAsync(ts => ts.NetSalary);

            // For now, we'll use salaries as the main expense component
            // In a real system, you'd have other expense tables
            var totalExpenses = totalSalaries;

            var netProfit = totalRevenue - totalExpenses;

            var paidStudents = await _context.StudentFees
                .Where(sf => sf.Status == FeeStatus.Paid &&
                    (!activeSessionId.HasValue || sf.Student.SessionId == activeSessionId))
                .Select(sf => sf.StudentId)
                .Distinct()
                .CountAsync();

            var pendingStudents = await _context.StudentFees
                .Where(sf => (sf.Status == FeeStatus.Pending || sf.Status == FeeStatus.PartiallyPaid) &&
                    (!activeSessionId.HasValue || sf.Student.SessionId == activeSessionId))
                .Select(sf => sf.StudentId)
                .Distinct()
                .CountAsync();

            var todaySummary = DateOnly.FromDateTime(DateTime.Today);
            var overdueStudents = await _context.StudentFees
                .Where(sf => sf.Status != FeeStatus.Paid && todaySummary > sf.DueDate &&
                    (!activeSessionId.HasValue || sf.Student.SessionId == activeSessionId))
                .Select(sf => sf.StudentId)
                .Distinct()
                .CountAsync();

            var summary = new FinancialSummary
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


            return Ok(new ApiResponse<FinancialSummary>
            {
                Success = true,
                Message = "Financial summary retrieved successfully",
                Data = summary
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving financial summary");
            return StatusCode(500, new ApiResponse<FinancialSummary>
            {
                Success = false,
                Message = "Error retrieving financial summary"
            });
        }
    }

    /// <summary>
    /// Get recent payments
    /// </summary>
    /// <param name="limit">Maximum number of payments to return</param>
    /// <returns>List of recent payments</returns>
    [HttpGet("payments")]
    //[Authorize]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PaymentDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PaymentDto>>>> GetRecentPayments([FromQuery] int limit = 10, [FromQuery] int? sessionId = null)
    {
        try
        {
            var targetSessionId = sessionId ?? await _context.Sessions.Where(s => s.IsActive && !s.IsDeleted).Select(s => (int?)s.Id).FirstOrDefaultAsync();
            var payments = await _context.Payments
                .Include(p => p.StudentFee)
                    .ThenInclude(sf => sf.Student)
                .Include(p => p.StudentFee.FeeStructure)
                .Where(p => p.Status == PaymentStatus.Completed &&
                    (!targetSessionId.HasValue || p.StudentFee.Student.SessionId == targetSessionId))
                .OrderByDescending(p => p.PaymentDate)
                .Take(limit)
                .Select(p => new PaymentDto
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
                })
                .ToListAsync();

            return Ok(new ApiResponse<IEnumerable<PaymentDto>>
            {
                Success = true,
                Message = "Recent payments retrieved successfully",
                Data = payments
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recent payments");
            return StatusCode(500, new ApiResponse<IEnumerable<PaymentDto>>
            {
                Success = false,
                Message = "Error retrieving recent payments"
            });
        }
    }

    /// <summary>
    /// Get payment history
    /// </summary>
    /// <param name="studentId">Optional student ID filter</param>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <returns>Payment history</returns>
    [HttpGet("payments/history")]
    //[Authorize]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PaymentDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PaymentDto>>>> GetPaymentHistory(
        [FromQuery] int? studentId,
        [FromQuery] int? sessionId,
        [FromQuery] string? startDate,
        [FromQuery] string? endDate)
    {
        try
        {
            var query = _context.Payments
                .Include(p => p.StudentFee)
                    .ThenInclude(sf => sf.Student)
                .Include(p => p.StudentFee.FeeStructure)
                .AsQueryable();

            var targetSessionId = sessionId ?? await _context.Sessions.Where(s => s.IsActive && !s.IsDeleted).Select(s => (int?)s.Id).FirstOrDefaultAsync();
            if (targetSessionId.HasValue)
            {
                query = query.Where(p => p.StudentFee.Student.SessionId == targetSessionId.Value);
            }

            if (studentId.HasValue)
            {
                query = query.Where(p => p.StudentFee.StudentId == studentId.Value);
            }

            if (!string.IsNullOrEmpty(startDate) && DateTime.TryParse(startDate, out var start))
            {
                query = query.Where(p => p.PaymentDate >= start);
            }

            if (!string.IsNullOrEmpty(endDate) && DateTime.TryParse(endDate, out var end))
            {
                query = query.Where(p => p.PaymentDate <= end);
            }

            var payments = await query
                .OrderByDescending(p => p.PaymentDate)
                .Select(p => new PaymentDto
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
                })
                .ToListAsync();

            return Ok(new ApiResponse<IEnumerable<PaymentDto>>
            {
                Success = true,
                Message = "Payment history retrieved successfully",
                Data = payments
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment history");
            return StatusCode(500, new ApiResponse<IEnumerable<PaymentDto>>
            {
                Success = false,
                Message = "Error retrieving payment history"
            });
        }
    }

    /// <summary>
    /// Create a new payment
    /// </summary>
    /// <param name="request">Payment creation request</param>
    /// <returns>Created payment</returns>
    [HttpPost("payments")]
    //[Authorize]
    [ProducesResponseType(typeof(ApiResponse<PaymentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<PaymentDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> CreatePayment([FromBody] CreatePaymentRequest request)
    {
        try
        {
            var studentFee = await _context.StudentFees
                .Include(sf => sf.Student)
                .Include(sf => sf.FeeStructure)
                .FirstOrDefaultAsync(sf => sf.Id == request.StudentFeeId);

            if (studentFee == null)
            {
                return BadRequest(new ApiResponse<PaymentDto>
                {
                    Success = false,
                    Message = "Student fee not found"
                });
            }

            if (request.Amount > studentFee.BalanceAmount)
            {
                return BadRequest(new ApiResponse<PaymentDto>
                {
                    Success = false,
                    Message = "Payment amount exceeds balance amount"
                });
            }

            var payment = new Payment
            {
                StudentFeeId = request.StudentFeeId,
                Amount = request.Amount,
                Method = request.Method,
                TransactionId = request.TransactionId,
                ReceiptNumber = request.InvoiceId,
                Remarks = request.Description,
                Status = PaymentStatus.Completed,
                ReceivedByUserId = GetCurrentUserId()
            };

            _context.Payments.Add(payment);

            // Update student fee paid amount and status
            studentFee.PaidAmount += request.Amount;
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

            var paymentDto = new PaymentDto
            {
                Id = payment.Id,
                StudentId = studentFee.StudentId,
                StudentName = $"{studentFee.Student.FirstName} {studentFee.Student.LastName}",
                Amount = payment.Amount,
                Date = payment.PaymentDate.ToString("yyyy-MM-dd"),
                Status = payment.Status.ToString(),
                Method = payment.Method.ToString(),
                Description = studentFee.FeeStructure.Name,
                InvoiceId = payment.ReceiptNumber
            };

            return CreatedAtAction(nameof(GetPaymentHistory), new { }, new ApiResponse<PaymentDto>
            {
                Success = true,
                Message = "Payment created successfully",
                Data = paymentDto
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment");
            return StatusCode(500, new ApiResponse<PaymentDto>
            {
                Success = false,
                Message = "Error creating payment"
            });
        }
    }

    /// <summary>
    /// Get pending payments
    /// </summary>
    /// <returns>List of pending payments</returns>
    [HttpGet("payments/pending")]
    //[Authorize]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PendingPaymentDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PendingPaymentDto>>>> GetPendingPayments()
    {
        try
        {
            var pendingPayments = await _context.StudentFees
                .Include(sf => sf.Student)
                .Include(sf => sf.FeeStructure)
                .Where(sf => sf.Status == FeeStatus.Pending || sf.Status == FeeStatus.PartiallyPaid)
                .Select(sf => new PendingPaymentDto
                {
                    Id = sf.Id,
                    StudentId = sf.StudentId,
                    StudentName = $"{sf.Student.FirstName} {sf.Student.LastName}",
                    FeeType = sf.FeeStructure.Name,
                    TotalAmount = sf.FinalAmount,
                    PaidAmount = sf.PaidAmount,
                    BalanceAmount = sf.BalanceAmount,
                    DueDate = sf.DueDate.ToString("yyyy-MM-dd"),
                    Status = sf.Status.ToString(),
                    IsOverdue = sf.IsOverdue
                })
                .ToListAsync();

            return Ok(new ApiResponse<IEnumerable<PendingPaymentDto>>
            {
                Success = true,
                Message = "Pending payments retrieved successfully",
                Data = pendingPayments
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending payments");
            return StatusCode(500, new ApiResponse<IEnumerable<PendingPaymentDto>>
            {
                Success = false,
                Message = "Error retrieving pending payments"
            });
        }
    }

    /// <summary>
    /// Generate financial report
    /// </summary>
    /// <param name="request">Report generation request</param>
    /// <returns>Generated report</returns>
    [HttpPost("reports/generate")]
    //[Authorize]
    [ProducesResponseType(typeof(ApiResponse<FinancialReport>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FinancialReport>>> GenerateReport([FromBody] GenerateReportRequest request)
    {
        try
        {
            if (!DateTime.TryParse(request.StartDate, out var startDate) ||
                !DateTime.TryParse(request.EndDate, out var endDate))
            {
                return BadRequest(new ApiResponse<FinancialReport>
                {
                    Success = false,
                    Message = "Invalid date format"
                });
            }

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
                    StartDate = request.StartDate,
                    EndDate = request.EndDate
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
                        ReceiptNumber = e.TransactionId
                    }).ToList()
                }
            };

            return Ok(new ApiResponse<FinancialReport>
            {
                Success = true,
                Message = "Financial report generated successfully",
                Data = report
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating financial report");
            return StatusCode(500, new ApiResponse<FinancialReport>
            {
                Success = false,
                Message = "Error generating financial report"
            });
        }
    }

    /// <summary>
    /// Get all fees
    /// </summary>
    /// <returns>List of all fees</returns>
    [HttpGet("fees")]
    //[Authorize]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<object>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<object>>>> GetFees()
    {
        try
        {
            await EnsureFinanceFeeSchemaAsync();

            var feeRows = await _context.StudentFees
                .AsNoTracking()
                .Include(sf => sf.Student)
                    .ThenInclude(s => s.StudentClasses)
                        .ThenInclude(sc => sc.Class)
                .Include(sf => sf.FeeStructure)
                .Where(sf => !sf.IsDeleted)
                .OrderByDescending(sf => sf.DueDate)
                .ToListAsync();

            var fees = feeRows.Select(sf =>
            {
                var studentClass = sf.Student?.StudentClasses?.FirstOrDefault(sc => sc.IsActive)
                    ?? sf.Student?.StudentClasses?.FirstOrDefault();
                return new
                {
                    sf.Id,
                    sf.StudentId,
                    StudentName = sf.Student != null ? $"{sf.Student.FirstName} {sf.Student.LastName}" : "Unknown",
                    ClassId = studentClass?.ClassId ?? 0,
                    ClassName = studentClass?.Class?.Name ?? "Not Assigned",
                    FeeType = sf.FeeStructure != null ? sf.FeeStructure.Type : FeeType.Tuition,
                    sf.Amount,
                    sf.PaidAmount,
                    BalanceAmount = sf.FinalAmount - sf.PaidAmount,
                    sf.DueDate,
                    Status = sf.Status.ToString(),
                    Description = sf.Remarks
                };
            }).ToList();

            return Ok(new ApiResponse<IEnumerable<object>>
            {
                Success = true,
                Data = fees,
                Message = "Fees retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving fees");
            return StatusCode(500, new ApiResponse<IEnumerable<object>>
            {
                Success = false,
                Message = "Error retrieving fees"
            });
        }
    }

    /// <summary>
    /// Get overdue fees
    /// </summary>
    /// <returns>List of overdue fees</returns>
    [HttpGet("fees/overdue")]
    //[Authorize]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<object>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<object>>>> GetOverdueFees()
    {
        try
        {
            await EnsureFinanceFeeSchemaAsync();

            var today = DateOnly.FromDateTime(DateTime.Today);

            var overdueFeeRows = await _context.StudentFees
                .AsNoTracking()
                .Include(sf => sf.Student)
                    .ThenInclude(s => s.StudentClasses)
                        .ThenInclude(sc => sc.Class)
                .Include(sf => sf.FeeStructure)
                .Where(sf => !sf.IsDeleted &&
                           sf.Status != FeeStatus.Paid &&
                           today > sf.DueDate)
                .OrderBy(sf => sf.DueDate)
                .ToListAsync();

            var overdueFees = overdueFeeRows.Select(sf =>
            {
                var studentClass = sf.Student?.StudentClasses?.FirstOrDefault(sc => sc.IsActive)
                    ?? sf.Student?.StudentClasses?.FirstOrDefault();
                return new
                {
                    sf.Id,
                    sf.StudentId,
                    StudentName = sf.Student != null ? $"{sf.Student.FirstName} {sf.Student.LastName}" : "Unknown",
                    ClassId = studentClass?.ClassId ?? 0,
                    ClassName = studentClass?.Class?.Name ?? "Not Assigned",
                    FeeType = sf.FeeStructure != null ? sf.FeeStructure.Type : FeeType.Tuition,
                    sf.Amount,
                    sf.PaidAmount,
                    BalanceAmount = sf.FinalAmount - sf.PaidAmount,
                    sf.DueDate,
                    Status = sf.Status.ToString(),
                    Description = sf.Remarks,
                    DaysOverdue = today.DayNumber - sf.DueDate.DayNumber
                };
            }).ToList();

            return Ok(new ApiResponse<IEnumerable<object>>
            {
                Success = true,
                Data = overdueFees,
                Message = "Overdue fees retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving overdue fees");
            return StatusCode(500, new ApiResponse<IEnumerable<object>>
            {
                Success = false,
                Message = "Error retrieving overdue fees"
            });
        }
    }

    /// <summary>
    /// Get all expenses
    /// </summary>
    /// <returns>List of all expenses</returns>
    [HttpGet("expenses")]
    //[Authorize]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<object>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<object>>>> GetExpenses()
    {
        try
        {
            // For now, return empty list as expenses are not implemented in the database yet
            var expenses = new List<object>();

            return Ok(new ApiResponse<IEnumerable<object>>
            {
                Success = true,
                Data = expenses,
                Message = "Expenses retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving expenses");
            return StatusCode(500, new ApiResponse<IEnumerable<object>>
            {
                Success = false,
                Message = "Error retrieving expenses"
            });
        }
    }

    /// <summary>
    /// Get all reports
    /// </summary>
    /// <returns>List of all reports</returns>
    [HttpGet("reports")]
    //[Authorize]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<object>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<object>>>> GetReports()
    {
        try
        {
            // For now, return empty list as reports are generated on-demand
            var reports = new List<object>();

            return Ok(new ApiResponse<IEnumerable<object>>
            {
                Success = true,
                Data = reports,
                Message = "Reports retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reports");
            return StatusCode(500, new ApiResponse<IEnumerable<object>>
            {
                Success = false,
                Message = "Error retrieving reports"
            });
        }
    }

    /// <summary>
    /// Create a new fee with proper fee type handling
    /// </summary>
    /// <param name="request">Fee creation request</param>
    /// <returns>Created fee details</returns>
    [HttpPost("fees")]
    //[Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<object>>> CreateFee([FromBody] CreateFeeRequest request)
    {
        try
        {
            await EnsureFinanceFeeSchemaAsync();

            if (!ModelState.IsValid)
            {
                var allErrors = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .Where(msg => !string.IsNullOrWhiteSpace(msg)));

                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = string.IsNullOrWhiteSpace(allErrors) ? "Invalid fee payload" : allErrors
                });
            }

            if (request.StudentId <= 0 || request.FeeStructureId <= 0)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Student and fee structure are required"
                });
            }

            if (request.Amount <= 0)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Fee amount must be greater than zero"
                });
            }

            if (!Enum.IsDefined(typeof(FeeCategory), request.FeeCategory))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid fee category. Use Monthly, FullCourse, or Additional."
                });
            }

            if (request.StartDate.HasValue && request.EndDate.HasValue && request.EndDate < request.StartDate)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "End date cannot be before start date"
                });
            }

            // Validate student exists
            var student = await _context.Students.FindAsync(request.StudentId);
            if (student == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Student not found"
                });
            }

            // Resolve fee structure; create fallback structure for legacy clients when needed.
            var feeStructure = await ResolveFeeStructureForCreateAsync(request);
            if (feeStructure == null)
            {
                var availableIds = await _context.FeeStructures
                    .AsNoTracking()
                    .Where(fs => fs.IsActive && !fs.IsDeleted)
                    .OrderBy(fs => fs.Id)
                    .Select(fs => fs.Id)
                    .ToListAsync();

                var availableHint = availableIds.Count == 0
                    ? "No active fee structures exist."
                    : $"Available feeStructureId values: {string.Join(", ", availableIds)}";

                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Fee structure not found. {availableHint}"
                });
            }

            var today = DateOnly.FromDateTime(DateTime.Today);
            var createdFees = new List<object>();

            // Handle different fee categories
            switch (request.FeeCategory)
            {
                case FeeCategory.Monthly:
                    // Create monthly recurring fee
                    var monthlyFee = await CreateMonthlyFee(request, student, feeStructure, today);
                    createdFees.Add(monthlyFee);
                    break;

                case FeeCategory.FullCourse:
                    // Create one-time course fee
                    var courseFee = await CreateCourseFee(request, student, feeStructure, today);
                    createdFees.Add(courseFee);
                    break;

                case FeeCategory.Additional:
                    // Create one-time additional fee
                    var additionalFee = await CreateAdditionalFee(request, student, feeStructure, today);
                    createdFees.Add(additionalFee);
                    break;
            }

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetFees), new ApiResponse<object>
            {
                Success = true,
                Data = createdFees,
                Message = "Fee created successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating fee");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = $"Error creating fee: {ex.Message}"
            });
        }
    }

    private async Task<object> CreateMonthlyFee(CreateFeeRequest request, Student student, FeeStructure feeStructure, DateOnly today)
    {
        var startDate = request.StartDate ?? today;
        var dueDay = request.RecurringDayOfMonth ?? startDate.Day;
        dueDay = Math.Clamp(dueDay, 1, 31);
        var endDate = request.EndDate ?? startDate.AddMonths(12); // Default 12 months

        // Create parent fee record for tracking
        var parentFee = new StudentFee
        {
            StudentId = request.StudentId,
            FeeStructureId = feeStructure.Id,
            Amount = request.Amount,
            DiscountAmount = request.DiscountAmount,
            FinalAmount = request.Amount - (request.DiscountAmount ?? 0),
            DueDate = BuildMonthDueDate(startDate.Year, startDate.Month, dueDay),
            FeeCategory = FeeCategory.Monthly,
            StartDate = startDate,
            EndDate = endDate,
            RecurringDayOfMonth = dueDay,
            IsRecurring = true,
            LateFeePerDay = request.LateFeePerDay ?? feeStructure.LateFeePerDay,
            GracePeriodDays = request.GracePeriodDays,
            Month = startDate.ToString("yyyy-MM"),
            AcademicYear = request.AcademicYear ?? GetCurrentAcademicYear(),
            Remarks = request.Remarks,
            Status = startDate <= today ? FeeStatus.Pending : FeeStatus.Pending
        };

        _context.StudentFees.Add(parentFee);
        await _context.SaveChangesAsync(); // Save to get the ID

        // Generate monthly fee instances for the specified period
        var monthlyFees = new List<object>();
        var currentDate = startDate;
        
        while (currentDate <= endDate)
        {
            var dueDate = BuildMonthDueDate(currentDate.Year, currentDate.Month, dueDay);
            
            // Skip if due date is in the past for future months
            if (dueDate < today && currentDate > today)
            {
                currentDate = currentDate.AddMonths(1);
                continue;
            }

            var monthlyFee = new StudentFee
            {
                StudentId = request.StudentId,
                FeeStructureId = feeStructure.Id,
                Amount = request.Amount,
                DiscountAmount = request.DiscountAmount,
                FinalAmount = request.Amount - (request.DiscountAmount ?? 0),
                DueDate = dueDate,
                FeeCategory = FeeCategory.Monthly,
                StartDate = startDate,
                EndDate = endDate,
                RecurringDayOfMonth = dueDay,
                IsRecurring = false, // Individual instances are not recurring
                ParentFeeId = parentFee.Id,
                LateFeePerDay = request.LateFeePerDay ?? feeStructure.LateFeePerDay,
                GracePeriodDays = request.GracePeriodDays,
                Month = currentDate.ToString("yyyy-MM"),
                AcademicYear = request.AcademicYear ?? GetCurrentAcademicYear(),
                Remarks = request.Remarks,
                Status = dueDate <= today ? FeeStatus.Pending : FeeStatus.Pending
            };

            monthlyFees.Add(new
            {
                monthlyFee.Id,
                monthlyFee.StudentId,
                StudentName = student.FirstName + " " + student.LastName,
                FeeType = feeStructure.Type.ToString(),
                monthlyFee.Amount,
                monthlyFee.FinalAmount,
                monthlyFee.DueDate,
                monthlyFee.Month,
                Status = monthlyFee.Status.ToString(),
                FeeCategory = "Monthly",
                IsRecurring = false,
                ParentFeeId = parentFee.Id
            });

            _context.StudentFees.Add(monthlyFee);
            currentDate = currentDate.AddMonths(1);
        }

        await _context.SaveChangesAsync();

        return new
        {
            parentFee.Id,
            parentFee.StudentId,
            StudentName = student.FirstName + " " + student.LastName,
            FeeType = feeStructure.Type.ToString(),
            parentFee.Amount,
            parentFee.FinalAmount,
            parentFee.StartDate,
            parentFee.EndDate,
            FeeCategory = "Monthly (Recurring)",
            IsRecurring = true,
            RecurringDayOfMonth = dueDay,
            GeneratedMonths = monthlyFees.Count,
            MonthlyInstances = monthlyFees
        };
    }

    private static DateOnly BuildMonthDueDate(int year, int month, int dueDay)
    {
        var maxDay = DateTime.DaysInMonth(year, month);
        var safeDay = Math.Min(Math.Max(dueDay, 1), maxDay);
        return new DateOnly(year, month, safeDay);
    }

    private async Task<FeeStructure?> ResolveFeeStructureForCreateAsync(CreateFeeRequest request)
    {
        var directMatch = await _context.FeeStructures
            .FirstOrDefaultAsync(fs => fs.Id == request.FeeStructureId && fs.IsActive && !fs.IsDeleted);
        if (directMatch != null)
        {
            return directMatch;
        }

        if (request.FeeStructureId <= 0)
        {
            return null;
        }

        (string Name, FeeType Type) catalog = LegacyFeeStructureCatalog.TryGetValue(request.FeeStructureId, out var mapped)
            ? mapped
            : ($"Adhoc Fee {request.FeeStructureId}", FeeType.Other);

        var existingByName = await _context.FeeStructures.FirstOrDefaultAsync(fs =>
            fs.IsActive && !fs.IsDeleted &&
            fs.Name == catalog.Name &&
            fs.Category == request.FeeCategory);

        if (existingByName != null)
        {
            return existingByName;
        }

        var fallback = new FeeStructure
        {
            Name = catalog.Name,
            Type = catalog.Type,
            Category = request.FeeCategory,
            Amount = request.Amount,
            Frequency = request.FeeCategory == FeeCategory.Monthly ? FeeFrequency.Monthly : FeeFrequency.OneTime,
            Description = "Auto-generated fee structure for compatibility",
            IsActive = true,
            AcademicYear = string.IsNullOrWhiteSpace(request.AcademicYear) ? GetCurrentAcademicYear() : request.AcademicYear,
            DurationMonths = request.FeeCategory == FeeCategory.FullCourse ? 12 : 0,
            LateFeePerDay = request.LateFeePerDay,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.FeeStructures.Add(fallback);
        await _context.SaveChangesAsync();
        return fallback;
    }

    private async Task<object> CreateCourseFee(CreateFeeRequest request, Student student, FeeStructure feeStructure, DateOnly today)
    {
        var startDate = request.StartDate ?? today;
        
        var courseFee = new StudentFee
        {
            StudentId = request.StudentId,
            FeeStructureId = feeStructure.Id,
            Amount = request.Amount,
            DiscountAmount = request.DiscountAmount,
            FinalAmount = request.Amount - (request.DiscountAmount ?? 0),
            DueDate = startDate, // Course fees are due immediately
            FeeCategory = FeeCategory.FullCourse,
            StartDate = startDate,
            EndDate = request.EndDate,
            IsRecurring = false,
            LateFeePerDay = request.LateFeePerDay ?? feeStructure.LateFeePerDay,
            GracePeriodDays = request.GracePeriodDays,
            AcademicYear = request.AcademicYear ?? GetCurrentAcademicYear(),
            Remarks = request.Remarks,
            Status = startDate <= today ? FeeStatus.Overdue : FeeStatus.Pending // Course fees are overdue immediately if start date is today or past
        };

        _context.StudentFees.Add(courseFee);
        await _context.SaveChangesAsync();

        return new
        {
            courseFee.Id,
            courseFee.StudentId,
            StudentName = student.FirstName + " " + student.LastName,
            FeeType = feeStructure.Type.ToString(),
            courseFee.Amount,
            courseFee.FinalAmount,
            courseFee.DueDate,
            courseFee.StartDate,
            courseFee.EndDate,
            Status = courseFee.Status.ToString(),
            FeeCategory = "Full Course",
            IsRecurring = false,
            IsOverdue = courseFee.IsOverdue,
            DaysOverdue = courseFee.DaysOverdue
        };
    }

    private async Task<object> CreateAdditionalFee(CreateFeeRequest request, Student student, FeeStructure feeStructure, DateOnly today)
    {
        var dueDate = request.DueDate ?? today.AddDays(7); // Default 7 days from today
        
        var additionalFee = new StudentFee
        {
            StudentId = request.StudentId,
            FeeStructureId = feeStructure.Id,
            Amount = request.Amount,
            DiscountAmount = request.DiscountAmount,
            FinalAmount = request.Amount - (request.DiscountAmount ?? 0),
            DueDate = dueDate,
            FeeCategory = FeeCategory.Additional,
            StartDate = today,
            IsRecurring = false,
            LateFeePerDay = request.LateFeePerDay ?? feeStructure.LateFeePerDay,
            GracePeriodDays = request.GracePeriodDays,
            AcademicYear = request.AcademicYear ?? GetCurrentAcademicYear(),
            Remarks = request.Remarks,
            Status = dueDate <= today ? FeeStatus.Pending : FeeStatus.Pending
        };

        _context.StudentFees.Add(additionalFee);
        await _context.SaveChangesAsync();

        return new
        {
            additionalFee.Id,
            additionalFee.StudentId,
            StudentName = student.FirstName + " " + student.LastName,
            FeeType = feeStructure.Type.ToString(),
            additionalFee.Amount,
            additionalFee.FinalAmount,
            additionalFee.DueDate,
            Status = additionalFee.Status.ToString(),
            FeeCategory = "Additional",
            IsRecurring = false,
            IsOverdue = additionalFee.IsOverdue,
            DaysOverdue = additionalFee.DaysOverdue
        };
    }

    private string GetCurrentAcademicYear()
    {
        var currentYear = DateTime.Now.Year;
        var currentMonth = DateTime.Now.Month;
        
        // Academic year typically starts in June
        return currentMonth >= 6 ? $"{currentYear}-{currentYear + 1}" : $"{currentYear - 1}-{currentYear}";
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    private async Task EnsureFinanceFeeSchemaAsync()
    {
        if (!_context.Database.IsSqlServer())
        {
            return;
        }

        await _context.Database.ExecuteSqlRawAsync(@"
IF COL_LENGTH('dbo.FeeStructures', 'Category') IS NULL
    ALTER TABLE dbo.FeeStructures ADD Category int NOT NULL CONSTRAINT DF_FeeStructures_Category DEFAULT(0);
IF COL_LENGTH('dbo.FeeStructures', 'DurationMonths') IS NULL
    ALTER TABLE dbo.FeeStructures ADD DurationMonths int NOT NULL CONSTRAINT DF_FeeStructures_DurationMonths DEFAULT(0);
IF COL_LENGTH('dbo.FeeStructures', 'LateFeePerDay') IS NULL
    ALTER TABLE dbo.FeeStructures ADD LateFeePerDay decimal(18,2) NULL;
IF COL_LENGTH('dbo.FeeStructures', 'DiscountAmount') IS NULL
    ALTER TABLE dbo.FeeStructures ADD DiscountAmount decimal(18,2) NULL;
IF COL_LENGTH('dbo.FeeStructures', 'DiscountDaysBeforeDue') IS NULL
    ALTER TABLE dbo.FeeStructures ADD DiscountDaysBeforeDue int NULL;
IF COL_LENGTH('dbo.FeeStructures', 'IsRefundable') IS NULL
    ALTER TABLE dbo.FeeStructures ADD IsRefundable bit NOT NULL CONSTRAINT DF_FeeStructures_IsRefundable DEFAULT(0);
IF COL_LENGTH('dbo.FeeStructures', 'RefundPercentage') IS NULL
    ALTER TABLE dbo.FeeStructures ADD RefundPercentage decimal(18,2) NULL;
IF COL_LENGTH('dbo.FeeStructures', 'TermsAndConditions') IS NULL
    ALTER TABLE dbo.FeeStructures ADD TermsAndConditions nvarchar(max) NULL;

IF COL_LENGTH('dbo.StudentFees', 'FeeCategory') IS NULL
    ALTER TABLE dbo.StudentFees ADD FeeCategory int NOT NULL CONSTRAINT DF_StudentFees_FeeCategory DEFAULT(0);
IF COL_LENGTH('dbo.StudentFees', 'StartDate') IS NULL
    ALTER TABLE dbo.StudentFees ADD StartDate date NULL;
IF COL_LENGTH('dbo.StudentFees', 'EndDate') IS NULL
    ALTER TABLE dbo.StudentFees ADD EndDate date NULL;
IF COL_LENGTH('dbo.StudentFees', 'RecurringDayOfMonth') IS NULL
    ALTER TABLE dbo.StudentFees ADD RecurringDayOfMonth int NULL;
IF COL_LENGTH('dbo.StudentFees', 'IsRecurring') IS NULL
    ALTER TABLE dbo.StudentFees ADD IsRecurring bit NOT NULL CONSTRAINT DF_StudentFees_IsRecurring DEFAULT(0);
IF COL_LENGTH('dbo.StudentFees', 'ParentFeeId') IS NULL
    ALTER TABLE dbo.StudentFees ADD ParentFeeId int NULL;
IF COL_LENGTH('dbo.StudentFees', 'LateFeePerDay') IS NULL
    ALTER TABLE dbo.StudentFees ADD LateFeePerDay decimal(18,2) NULL;
IF COL_LENGTH('dbo.StudentFees', 'GracePeriodDays') IS NULL
    ALTER TABLE dbo.StudentFees ADD GracePeriodDays int NOT NULL CONSTRAINT DF_StudentFees_GracePeriodDays DEFAULT(0);
IF COL_LENGTH('dbo.StudentFees', 'Month') IS NULL
    ALTER TABLE dbo.StudentFees ADD [Month] nvarchar(20) NULL;
IF COL_LENGTH('dbo.StudentFees', 'AcademicYear') IS NULL
    ALTER TABLE dbo.StudentFees ADD AcademicYear nvarchar(20) NOT NULL CONSTRAINT DF_StudentFees_AcademicYear DEFAULT('');
");
    }
}

// DTOs
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

public class PendingPaymentDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string FeeType { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceAmount { get; set; }
    public string DueDate { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsOverdue { get; set; }
}

public class CreatePaymentRequest
{
    public int StudentFeeId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public string? TransactionId { get; set; }
    public string? InvoiceId { get; set; }
    public string? Description { get; set; }
}

public class GenerateReportRequest
{
    public string StartDate { get; set; } = string.Empty;
    public string EndDate { get; set; } = string.Empty;
}

public class CreateFeeRequest
{
    public int StudentId { get; set; }
    public int FeeStructureId { get; set; }
    public decimal Amount { get; set; }
    public decimal? DiscountAmount { get; set; }
    public FeeCategory FeeCategory { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public DateOnly? DueDate { get; set; }
    public int? RecurringDayOfMonth { get; set; }
    public decimal? LateFeePerDay { get; set; }
    public int GracePeriodDays { get; set; } = 0;
    public string AcademicYear { get; set; } = string.Empty;
    public string? Remarks { get; set; }
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
