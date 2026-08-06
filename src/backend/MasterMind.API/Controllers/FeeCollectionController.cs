using MasterMind.API.Data;
using MasterMind.API.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text;
using MasterMind.API.Services.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace MasterMind.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class FeeCollectionController : ControllerBase
{
    private readonly MasterMindDbContext _context;
    private readonly ILogger<FeeCollectionController> _logger;
    private readonly IEmailService _emailService;

    public FeeCollectionController(
        MasterMindDbContext context,
        ILogger<FeeCollectionController> logger,
        IEmailService emailService)
    {
        _context = context;
        _logger = logger;
        _emailService = emailService;
    }

    /// <summary>
    /// Get all fee collections/payments
    /// </summary>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<object>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<object>>>> GetFeeCollections([FromQuery] int? sessionId = null)
    {
        try
        {
            if (!sessionId.HasValue)
            {
                var activeSession = await _context.Sessions.FirstOrDefaultAsync(s => s.IsActive && !s.IsDeleted);
                sessionId = activeSession?.Id;
            }

            var payments = await _context.Payments
                .AsSplitQuery()
                .Include(p => p.StudentFee)
                    .ThenInclude(sf => sf!.Student)
                .ThenInclude(s => s.Session)
                .Where(p => !sessionId.HasValue || p.StudentFee.Student.SessionId == sessionId.Value)
                .OrderByDescending(p => p.PaymentDate)
                .Take(100)
                .ToListAsync();

            var rows = payments.Select(p => new
            {
                p.Id,
                p.Amount,
                p.PaymentDate,
                PaymentMethod = p.Method.ToString(),
                p.TransactionId,
                p.ReceiptNumber,
                StudentName = p.StudentFee?.Student != null
                    ? $"{p.StudentFee.Student.FirstName} {p.StudentFee.Student.LastName}"
                    : "Unknown"
            }).ToList();

            return Ok(new ApiResponse<IEnumerable<object>>
            {
                Success = true,
                Message = "Fee collections retrieved successfully",
                Data = rows
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving fee collections");
            return Ok(new ApiResponse<IEnumerable<object>>
            {
                Success = true,
                Message = "No fee collections found",
                Data = new List<object>()
            });
        }
    }

    /// <summary>
    /// Create fee structure for a student (Monthly or Full Course)
    /// </summary>
    /// <param name="request">Fee setup request</param>
    /// <returns>Created fee structure</returns>
    [HttpPost("setup-student-fee")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<StudentFeeSetupDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<StudentFeeSetupDto>>> SetupStudentFee([FromBody] SetupStudentFeeRequest request)
    {
        try
        {
            var student = await _context.Students
                .Include(s => s.StudentClasses)
                    .ThenInclude(sc => sc.Class)
                .FirstOrDefaultAsync(s => s.Id == request.StudentId);

            if (student == null)
            {
                return BadRequest(new ApiResponse<StudentFeeSetupDto>
                {
                    Success = false,
                    Message = "Student not found"
                });
            }

            var feeStructure = await _context.FeeStructures
                .FirstOrDefaultAsync(fs => fs.Id == request.FeeStructureId);

            if (feeStructure == null)
            {
                return BadRequest(new ApiResponse<StudentFeeSetupDto>
                {
                    Success = false,
                    Message = "Fee structure not found"
                });
            }

            // Create payment schedule based on fee category
            if (feeStructure.Category == FeeCategory.Monthly)
            {
                return await CreateMonthlyFeeSchedule(student, feeStructure, request);
            }
            else if (feeStructure.Category == FeeCategory.FullCourse)
            {
                return await CreateFullCourseFee(student, feeStructure, request);
            }
            else
            {
                return BadRequest(new ApiResponse<StudentFeeSetupDto>
                {
                    Success = false,
                    Message = "Invalid fee category"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting up student fee");
            return StatusCode(500, new ApiResponse<StudentFeeSetupDto>
            {
                Success = false,
                Message = "Error setting up student fee"
            });
        }
    }

    /// <summary>
    /// Collect fee payment and generate receipt
    /// </summary>
    /// <param name="request">Payment collection request</param>
    /// <returns>Payment receipt</returns>
    [HttpPost("collect-payment")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<FeeReceiptDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<FeeReceiptDto>>> CollectPayment([FromBody] CollectPaymentRequest request)
    {
        try
        {
            if (request.FeeItems.Count == 0)
            {
                return BadRequest(new ApiResponse<FeeReceiptDto>
                {
                    Success = false,
                    Message = "Select at least one fee installment to collect"
                });
            }

            var student = await _context.Students
                .Include(s => s.StudentClasses)
                    .ThenInclude(sc => sc.Class)
                .FirstOrDefaultAsync(s => s.Id == request.StudentId);

            if (student == null)
            {
                return BadRequest(new ApiResponse<FeeReceiptDto>
                {
                    Success = false,
                    Message = "Student not found"
                });
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            // Process payment for each fee item
            var payments = new List<Payment>();
            var receiptItems = new List<FeeReceiptItem>();
            decimal totalSelectedAmount = 0;
            decimal totalRemainingBalance = 0;
            var suppliedTransactionId = string.IsNullOrWhiteSpace(request.TransactionId)
                ? null
                : request.TransactionId.Trim();

            foreach (var feeItem in request.FeeItems.GroupBy(item => item.StudentFeeId).Select(group => group.First()))
            {
                var studentFee = await _context.StudentFees
                    .Include(sf => sf.FeeStructure)
                    .FirstOrDefaultAsync(sf => sf.Id == feeItem.StudentFeeId &&
                        sf.StudentId == request.StudentId && !sf.IsDeleted && !sf.IsRecurring);

                if (studentFee == null)
                {
                    return BadRequest(new ApiResponse<FeeReceiptDto>
                    {
                        Success = false,
                        Message = $"Student fee with ID {feeItem.StudentFeeId} not found"
                    });
                }
                var remaining = studentFee.FinalAmount - studentFee.PaidAmount;
                if (feeItem.Amount <= 0 || feeItem.Amount > remaining)
                {
                    return BadRequest(new ApiResponse<FeeReceiptDto>
                    {
                        Success = false,
                        Message = $"Payment for fee {studentFee.Id} must be between 0 and {remaining:0.00}"
                    });
                }

                // Create payment
                var payment = new Payment
                {
                    StudentFeeId = studentFee.Id,
                    Amount = feeItem.Amount,
                    Method = request.PaymentMethod,
                    TransactionId = suppliedTransactionId,
                    ReceiptNumber = $"REC-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}"[..35],
                    Remarks = request.Remarks,
                    Status = PaymentStatus.Completed,
                    ReceivedByUserId = GetCurrentUserId()
                };

                _context.Payments.Add(payment);
                payments.Add(payment);

                // Update student fee
                studentFee.PaidAmount += feeItem.Amount;
                if (studentFee.PaidAmount >= studentFee.FinalAmount)
                {
                    studentFee.Status = FeeStatus.Paid;
                    studentFee.PaidAmount = studentFee.FinalAmount;
                }
                else
                {
                    studentFee.Status = FeeStatus.PartiallyPaid;
                }
                totalSelectedAmount += studentFee.FinalAmount;
                totalRemainingBalance += studentFee.FinalAmount - studentFee.PaidAmount;

                // Create receipt item
                var receiptItem = new FeeReceiptItem
                {
                    ItemDescription = $"{studentFee.FeeStructure.Name} - {feeItem.Description}",
                    ItemAmount = studentFee.FinalAmount,
                    DiscountAmount = feeItem.DiscountAmount,
                    FinalAmount = feeItem.Amount,
                    Period = feeItem.Period,
                    StudentFeeId = studentFee.Id
                };

                receiptItems.Add(receiptItem);
            }

            // Persist payments first so a blank external reference can safely fall back to
            // the database-generated, monotonically increasing payment ID. This remains
            // inside the transaction, so a later receipt failure still rolls everything back.
            await _context.SaveChangesAsync();
            if (suppliedTransactionId == null)
            {
                foreach (var payment in payments)
                {
                    payment.TransactionId = $"MM-PAY-{payment.Id:D8}";
                }
            }

            // Generate receipt
            var receipt = new FeeReceipt
            {
                ReceiptNumber = $"RCP-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}"[..35],
                StudentId = request.StudentId,
                TotalAmount = totalSelectedAmount,
                PaidAmount = payments.Sum(payment => payment.Amount),
                BalanceAmount = totalRemainingBalance,
                PaymentMethod = request.PaymentMethod.ToString(),
                Payment = payments.First(),
                StudentName = $"{student.FirstName} {student.LastName}",
                StudentClass = student.StudentClasses.FirstOrDefault()?.Class?.Name ?? "N/A",
                FeeDescription = string.Join(", ", request.FeeItems.Select(fi => fi.Description)),
                FeePeriod = request.FeeItems.FirstOrDefault()?.Period ?? "",
                ParentName = FirstNonBlank(student.MotherName, student.FatherName, student.ParentName, "Parent/Guardian"),
                ParentEmail = student.ParentEmail ?? string.Empty,
                ParentMobile = student.ParentMobile ?? string.Empty,
                GeneratedByUserId = GetCurrentUserId(),
                InstitutionName = "MasterMind Coaching Classes",
                InstitutionAddress = "Kedia Palace, Sikar, Rajasthan",
                InstitutionContact = "9887258679",
                ReceiptItems = receiptItems
            };

            _context.FeeReceipts.Add(receipt);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            try
            {
                await LogFeeReceiptTemplateUsageAsync(receipt);
            }
            catch (Exception logException)
            {
                _logger.LogWarning(logException,
                    "Payment {ReceiptNumber} was saved, but Template Zone logging could not be completed",
                    receipt.ReceiptNumber);
            }

            // Map to DTO
            var receiptDto = new FeeReceiptDto
            {
                Id = receipt.Id,
                ReceiptNumber = receipt.ReceiptNumber,
                StudentName = receipt.StudentName,
                StudentClass = receipt.StudentClass,
                TotalAmount = receipt.TotalAmount,
                PaidAmount = receipt.PaidAmount,
                BalanceAmount = receipt.BalanceAmount,
                PaymentMethod = receipt.PaymentMethod,
                ReceiptDate = receipt.ReceiptDate.ToString("yyyy-MM-dd HH:mm:ss"),
                FeeDescription = receipt.FeeDescription,
                FeePeriod = receipt.FeePeriod,
                ParentName = receipt.ParentName,
                ParentEmail = receipt.ParentEmail,
                ParentMobile = receipt.ParentMobile,
                ReceiptItems = receipt.ReceiptItems.Select(ri => new FeeReceiptItemDto
                {
                    ItemDescription = ri.ItemDescription,
                    ItemAmount = ri.ItemAmount,
                    DiscountAmount = ri.DiscountAmount,
                    FinalAmount = ri.FinalAmount,
                    Period = ri.Period
                }).ToList()
            };

            _logger.LogInformation($"Payment collected and receipt generated: {receipt.ReceiptNumber} for {receipt.StudentName}");

            return CreatedAtAction(nameof(GetReceipt), new { id = receipt.Id }, new ApiResponse<FeeReceiptDto>
            {
                Success = true,
                Message = "Payment collected successfully and receipt generated",
                Data = receiptDto
            });
        }
        catch (Exception ex)
        {
            var reference = HttpContext.TraceIdentifier;
            var rootCause = ex.GetBaseException();
            var diagnosticCode = rootCause is SqlException sqlException
                ? $"DB-{sqlException.Number}"
                : ex is DbUpdateException
                    ? "DB-WRITE"
                    : "PAYMENT-WRITE";
            _logger.LogError(ex,
                "Error collecting payment. Reference {Reference}; diagnostic {DiagnosticCode}",
                reference,
                diagnosticCode);
            return StatusCode(500, new ApiResponse<FeeReceiptDto>
            {
                Success = false,
                Message = $"Payment could not be saved. No fee balance was changed. Support reference: {reference} ({diagnosticCode})."
            });
        }
    }

    private static string FirstNonBlank(params string?[] values) =>
        values.First(value => !string.IsNullOrWhiteSpace(value))!.Trim();

    private async Task LogFeeReceiptTemplateUsageAsync(FeeReceipt receipt)
    {
        var template = await _context.MessageTemplates
            .Where(t => !t.IsDeleted && t.IsActive && t.Type == TemplateType.FeeReceipt)
            .OrderByDescending(t => t.UpdatedAt ?? t.CreatedAt)
            .FirstOrDefaultAsync();

        if (template == null)
        {
            return;
        }

        var tokens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["ReceiptNumber"] = receipt.ReceiptNumber,
            ["ReceiptDate"] = receipt.ReceiptDate.ToString("yyyy-MM-dd HH:mm:ss"),
            ["StudentName"] = receipt.StudentName,
            ["ParentName"] = receipt.ParentName,
            ["ClassName"] = receipt.StudentClass,
            ["FeeAmount"] = receipt.PaidAmount.ToString("0.00")
        };

        var renderedSubject = template.Subject;
        var renderedBody = template.Body;
        foreach (var token in tokens)
        {
            renderedSubject = renderedSubject.Replace($"{{{{{token.Key}}}}}", token.Value, StringComparison.OrdinalIgnoreCase);
            renderedBody = renderedBody.Replace($"{{{{{token.Key}}}}}", token.Value, StringComparison.OrdinalIgnoreCase);
        }

        var log = new TemplateDispatchLog
        {
            MessageTemplateId = template.Id,
            StudentId = receipt.StudentId,
            FeeReceiptId = receipt.Id,
            Channel = "System",
            Status = "Generated",
            RenderedSubject = renderedSubject,
            RenderedBody = renderedBody,
            GeneratedAt = DateTime.UtcNow
        };

        _context.TemplateDispatchLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Get fee receipt by ID
    /// </summary>
    /// <param name="id">Receipt ID</param>
    /// <returns>Fee receipt</returns>
    [HttpGet("receipt/{id}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<FeeReceiptDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FeeReceiptDto>>> GetReceipt(int id)
    {
        try
        {
            var receipt = await _context.FeeReceipts
                .Include(fr => fr.ReceiptItems)
                .FirstOrDefaultAsync(fr => fr.Id == id);

            if (receipt == null)
            {
                return NotFound(new ApiResponse<FeeReceiptDto>
                {
                    Success = false,
                    Message = "Receipt not found"
                });
            }

            var receiptDto = new FeeReceiptDto
            {
                Id = receipt.Id,
                ReceiptNumber = receipt.ReceiptNumber,
                StudentName = receipt.StudentName,
                StudentClass = receipt.StudentClass,
                TotalAmount = receipt.TotalAmount,
                PaidAmount = receipt.PaidAmount,
                BalanceAmount = receipt.BalanceAmount,
                PaymentMethod = receipt.PaymentMethod,
                ReceiptDate = receipt.ReceiptDate.ToString("yyyy-MM-dd HH:mm:ss"),
                FeeDescription = receipt.FeeDescription,
                FeePeriod = receipt.FeePeriod,
                ParentName = receipt.ParentName,
                ParentEmail = receipt.ParentEmail,
                ParentMobile = receipt.ParentMobile,
                IsEmailSent = receipt.IsEmailSent,
                IsSmsSent = receipt.IsSmsSent,
                ReceiptItems = receipt.ReceiptItems.Select(ri => new FeeReceiptItemDto
                {
                    ItemDescription = ri.ItemDescription,
                    ItemAmount = ri.ItemAmount,
                    DiscountAmount = ri.DiscountAmount,
                    FinalAmount = ri.FinalAmount,
                    Period = ri.Period
                }).ToList()
            };

            return Ok(new ApiResponse<FeeReceiptDto>
            {
                Success = true,
                Message = "Receipt retrieved successfully",
                Data = receiptDto
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving receipt");
            return StatusCode(500, new ApiResponse<FeeReceiptDto>
            {
                Success = false,
                Message = "Error retrieving receipt"
            });
        }
    }

    /// <summary>
    /// Send receipt via email
    /// </summary>
    /// <param name="id">Receipt ID</param>
    /// <returns>Success response</returns>
    [HttpPost("receipt/{id}/send-email")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> SendReceiptEmail(int id)
    {
        try
        {
            var receipt = await _context.FeeReceipts
                .Include(fr => fr.ReceiptItems)
                .FirstOrDefaultAsync(fr => fr.Id == id);

            if (receipt == null)
            {
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Receipt not found"
                });
            }

            if (string.IsNullOrWhiteSpace(receipt.ParentEmail))
            {
                return BadRequest(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "The parent has not supplied a recovery email"
                });
            }
            var body = $"<p>Namaste {receipt.ParentName},</p>" +
                $"<p>Payment receipt <strong>{receipt.ReceiptNumber}</strong> for " +
                $"<strong>₹{receipt.PaidAmount:N2}</strong> has been generated for {receipt.StudentName}.</p>" +
                "<p>You can sign in to the Parent portal to view fee balances and receipt details.</p>";
            var sent = await _emailService.SendEmailAsync(
                receipt.ParentEmail,
                $"Fee receipt {receipt.ReceiptNumber}",
                body);
            if (!sent)
            {
                return StatusCode(502, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Email provider did not accept the receipt email"
                });
            }
            receipt.IsEmailSent = true;
            receipt.EmailSentAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Receipt {receipt.ReceiptNumber} sent via email to {receipt.ParentEmail}");

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = "Receipt sent successfully via email",
                Data = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending receipt email");
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = "Error sending receipt email"
            });
        }
    }

    [HttpGet("receipt/{id}/pdf")]
    [Produces("application/pdf")]
    public async Task<IActionResult> DownloadReceiptPdf(int id)
    {
        var receipt = await _context.FeeReceipts
            .Include(r => r.ReceiptItems)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
        if (receipt == null) return NotFound();

        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(45);
                page.Header().Text(receipt.InstitutionName ?? "MasterMind Coaching Classes").FontSize(20).Bold();
                page.Content().PaddingVertical(20).Column(column =>
                {
                    column.Spacing(7);
                    column.Item().Text("FEE PAYMENT RECEIPT").FontSize(15).Bold();
                    column.Item().Text($"Receipt: {receipt.ReceiptNumber}");
                    column.Item().Text($"Student: {receipt.StudentName} — {receipt.StudentClass}");
                    column.Item().Text($"Parent: {receipt.ParentName}");
                    column.Item().Text($"Period: {receipt.FeePeriod}");
                    foreach (var item in receipt.ReceiptItems)
                        column.Item().Text($"{item.ItemDescription}: ₹{item.FinalAmount:N2}");
                    column.Item().PaddingTop(8).Text($"Amount paid: ₹{receipt.PaidAmount:N2}").FontSize(15).Bold();
                    column.Item().Text($"Balance: ₹{receipt.BalanceAmount:N2}");
                    column.Item().Text($"Method: {receipt.PaymentMethod}");
                    column.Item().Text($"Date: {receipt.ReceiptDate:dd MMMM yyyy}");
                    column.Item().PaddingTop(18).Text("Status: PAID").Bold().FontColor(Colors.Green.Darken2);
                });
                page.Footer().AlignCenter().Text("This is a system-generated receipt.");
            });
        }).GeneratePdf();
        return File(pdf, "application/pdf", $"Fee-Receipt-{receipt.ReceiptNumber}.pdf");
    }

    /// <summary>
    /// Get student fee details for payment collection
    /// </summary>
    /// <param name="studentId">Student ID</param>
    /// <returns>Student fee details</returns>
    [HttpGet("student/{studentId}/fee-details")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<StudentFeeDetailsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<StudentFeeDetailsDto>>> GetStudentFeeDetails(int studentId)
    {
        try
        {
            var student = await _context.Students
                .Include(s => s.StudentClasses)
                    .ThenInclude(sc => sc.Class)
                .FirstOrDefaultAsync(s => s.Id == studentId && !s.IsDeleted);

            if (student == null)
            {
                return NotFound(new ApiResponse<StudentFeeDetailsDto>
                {
                    Success = false,
                    Message = "Student not found"
                });
            }

            var pendingFees = await _context.StudentFees
                .AsNoTracking()
                .Include(sf => sf.FeeStructure)
                .Where(sf => sf.StudentId == studentId
                    && !sf.IsDeleted
                    && !sf.IsRecurring
                    && (!sf.ParentFeeId.HasValue || !sf.ParentFee!.IsDeleted)
                    && sf.Status != FeeStatus.Paid
                    && sf.Status != FeeStatus.Waived
                    && sf.Status != FeeStatus.Cancelled
                    && sf.FinalAmount > sf.PaidAmount)
                .OrderBy(sf => sf.DueDate)
                .ThenBy(sf => sf.Id)
                .ToListAsync();

            var feeDetails = new StudentFeeDetailsDto
            {
                StudentId = student.Id,
                StudentName = $"{student.FirstName} {student.LastName}",
                StudentClass = student.StudentClasses.FirstOrDefault()?.Class?.Name ?? "N/A",
                ParentName = student.ParentName,
                ParentEmail = student.ParentEmail,
                ParentMobile = student.ParentMobile,
                PendingFees = pendingFees
                    .Select(sf => new PendingFeeItemDto
                    {
                        StudentFeeId = sf.Id,
                        FeeType = sf.FeeStructure.Name,
                        FeeCategory = sf.FeeStructure.Category.ToString(),
                        Amount = sf.FinalAmount,
                        PaidAmount = sf.PaidAmount,
                        BalanceAmount = sf.BalanceAmount,
                        DueDate = sf.DueDate.ToString("yyyy-MM-dd"),
                        Month = sf.Month,
                        IsOverdue = sf.IsOverdue
                    }).ToList()
            };

            return Ok(new ApiResponse<StudentFeeDetailsDto>
            {
                Success = true,
                Message = "Student fee details retrieved successfully",
                Data = feeDetails
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving student fee details");
            return StatusCode(500, new ApiResponse<StudentFeeDetailsDto>
            {
                Success = false,
                Message = "Error retrieving student fee details"
            });
        }
    }

    private async Task<ActionResult<ApiResponse<StudentFeeSetupDto>>> CreateMonthlyFeeSchedule(Student student, FeeStructure feeStructure, SetupStudentFeeRequest request)
    {
        // Create payment schedule for monthly fees
        var schedule = new FeePaymentSchedule
        {
            StudentId = student.Id,
            FeeStructureId = feeStructure.Id,
            ScheduleType = "Monthly",
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            MonthlyAmount = feeStructure.Amount,
            TotalInstallments = request.NumberOfMonths ?? 12,
            AcademicYear = request.AcademicYear
        };

        _context.FeePaymentSchedules.Add(schedule);

        // Create monthly installments
        for (int i = 0; i < schedule.TotalInstallments; i++)
        {
            var dueDate = request.StartDate.AddMonths(i);
            var studentFee = new StudentFee
            {
                StudentId = student.Id,
                FeeStructureId = feeStructure.Id,
                Amount = feeStructure.Amount,
                FinalAmount = feeStructure.Amount,
                DueDate = DateOnly.FromDateTime(dueDate),
                Status = FeeStatus.Pending,
                Month = dueDate.ToString("MMMM yyyy"),
                AcademicYear = request.AcademicYear
                // BalanceAmount is computed automatically: FinalAmount - PaidAmount
            };

            _context.StudentFees.Add(studentFee);

            var installment = new FeeInstallment
            {
                FeePaymentScheduleId = schedule.Id,
                InstallmentNumber = i + 1,
                Amount = feeStructure.Amount,
                DueDate = dueDate,
                Status = "Pending",
                StudentFee = studentFee
            };

            _context.FeeInstallments.Add(installment);
        }

        await _context.SaveChangesAsync();

        var setupDto = new StudentFeeSetupDto
        {
            StudentId = student.Id,
            StudentName = $"{student.FirstName} {student.LastName}",
            FeeType = "Monthly",
            TotalAmount = schedule.MonthlyAmount * schedule.TotalInstallments,
            MonthlyAmount = schedule.MonthlyAmount,
            NumberOfInstallments = schedule.TotalInstallments,
            StartDate = schedule.StartDate.ToString("yyyy-MM-dd"),
            EndDate = schedule.EndDate?.ToString("yyyy-MM-dd"),
            Status = "Active"
        };

        return CreatedAtAction(nameof(GetStudentFeeDetails), new { studentId = student.Id }, new ApiResponse<StudentFeeSetupDto>
        {
            Success = true,
            Message = "Monthly fee schedule created successfully",
            Data = setupDto
        });
    }

    private async Task<ActionResult<ApiResponse<StudentFeeSetupDto>>> CreateFullCourseFee(Student student, FeeStructure feeStructure, SetupStudentFeeRequest request)
    {
        // Create single fee for full course
        var studentFee = new StudentFee
        {
            StudentId = student.Id,
            FeeStructureId = feeStructure.Id,
            Amount = feeStructure.Amount,
            FinalAmount = feeStructure.Amount,
            DueDate = DateOnly.FromDateTime(request.DueDate),
            Status = FeeStatus.Pending,
            AcademicYear = request.AcademicYear,
            // BalanceAmount is computed automatically: FinalAmount - PaidAmount
            Remarks = "Full Course Fee"
        };

        _context.StudentFees.Add(studentFee);
        await _context.SaveChangesAsync();

        var setupDto = new StudentFeeSetupDto
        {
            StudentId = student.Id,
            StudentName = $"{student.FirstName} {student.LastName}",
            FeeType = "Full Course",
            TotalAmount = feeStructure.Amount,
            MonthlyAmount = 0,
            NumberOfInstallments = 1,
            StartDate = DateTime.Today.ToString("yyyy-MM-dd"),
            EndDate = request.DueDate.ToString("yyyy-MM-dd"),
            Status = "Pending"
        };

        return CreatedAtAction(nameof(GetStudentFeeDetails), new { studentId = student.Id }, new ApiResponse<StudentFeeSetupDto>
        {
            Success = true,
            Message = "Full course fee created successfully",
            Data = setupDto
        });
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}

// DTOs
public class SetupStudentFeeRequest
{
    public int StudentId { get; set; }
    public int FeeStructureId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime DueDate { get; set; }
    public int? NumberOfMonths { get; set; }
    public string AcademicYear { get; set; } = string.Empty;
}

public class StudentFeeSetupDto
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string FeeType { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal MonthlyAmount { get; set; }
    public int NumberOfInstallments { get; set; }
    public string StartDate { get; set; } = string.Empty;
    public string? EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class CollectPaymentRequest
{
    public int StudentId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? TransactionId { get; set; }
    public string? Remarks { get; set; }
    public List<PaymentFeeItemDto> FeeItems { get; set; } = new();
}

public class PaymentFeeItemDto
{
    public int StudentFeeId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal ItemAmount { get; set; }
    public decimal Amount { get; set; }
    public decimal? DiscountAmount { get; set; }
    public string Period { get; set; } = string.Empty;
}

public class FeeReceiptDto
{
    public int Id { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string StudentClass { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceAmount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string ReceiptDate { get; set; } = string.Empty;
    public string FeeDescription { get; set; } = string.Empty;
    public string FeePeriod { get; set; } = string.Empty;
    public string ParentName { get; set; } = string.Empty;
    public string ParentEmail { get; set; } = string.Empty;
    public string ParentMobile { get; set; } = string.Empty;
    public bool IsEmailSent { get; set; }
    public bool IsSmsSent { get; set; }
    public List<FeeReceiptItemDto> ReceiptItems { get; set; } = new();
}

public class FeeReceiptItemDto
{
    public string ItemDescription { get; set; } = string.Empty;
    public decimal ItemAmount { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public string Period { get; set; } = string.Empty;
}

public class StudentFeeDetailsDto
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentClass { get; set; } = string.Empty;
    public string ParentName { get; set; } = string.Empty;
    public string ParentEmail { get; set; } = string.Empty;
    public string ParentMobile { get; set; } = string.Empty;
    public List<PendingFeeItemDto> PendingFees { get; set; } = new();
}

public class PendingFeeItemDto
{
    public int StudentFeeId { get; set; }
    public string FeeType { get; set; } = string.Empty;
    public string FeeCategory { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceAmount { get; set; }
    public string DueDate { get; set; } = string.Empty;
    public string? Month { get; set; }
    public bool IsOverdue { get; set; }
}
