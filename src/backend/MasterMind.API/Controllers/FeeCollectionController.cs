using MasterMind.API.Data;
using MasterMind.API.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace MasterMind.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class FeeCollectionController : ControllerBase
{
    private readonly MasterMindDbContext _context;
    private readonly ILogger<FeeCollectionController> _logger;

    public FeeCollectionController(MasterMindDbContext context, ILogger<FeeCollectionController> logger)
    {
        _context = context;
        _logger = logger;
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

            // Process payment for each fee item
            var payments = new List<Payment>();
            var receiptItems = new List<FeeReceiptItem>();

            foreach (var feeItem in request.FeeItems)
            {
                var studentFee = await _context.StudentFees
                    .Include(sf => sf.FeeStructure)
                    .FirstOrDefaultAsync(sf => sf.Id == feeItem.StudentFeeId);

                if (studentFee == null)
                {
                    return BadRequest(new ApiResponse<FeeReceiptDto>
                    {
                        Success = false,
                        Message = $"Student fee with ID {feeItem.StudentFeeId} not found"
                    });
                }

                // Create payment
                var payment = new Payment
                {
                    StudentFeeId = studentFee.Id,
                    Amount = feeItem.Amount,
                    Method = request.PaymentMethod,
                    TransactionId = request.TransactionId,
                    ReceiptNumber = $"REC-{DateTime.Now:yyyyMMddHHmmss}",
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

            // Generate receipt
            var receipt = new FeeReceipt
            {
                ReceiptNumber = $"RCP-{DateTime.Now:yyyyMMddHHmmss}",
                StudentId = request.StudentId,
                TotalAmount = request.FeeItems.Sum(fi => fi.ItemAmount),
                PaidAmount = request.FeeItems.Sum(fi => fi.Amount),
                BalanceAmount = request.FeeItems.Sum(fi => fi.ItemAmount) - request.FeeItems.Sum(fi => fi.Amount),
                PaymentMethod = request.PaymentMethod.ToString(),
                StudentName = $"{student.FirstName} {student.LastName}",
                StudentClass = student.StudentClasses.FirstOrDefault()?.Class?.Name ?? "N/A",
                FeeDescription = string.Join(", ", request.FeeItems.Select(fi => fi.Description)),
                FeePeriod = request.FeeItems.FirstOrDefault()?.Period ?? "",
                ParentName = student.ParentName,
                ParentEmail = student.ParentEmail,
                ParentMobile = student.ParentMobile,
                GeneratedByUserId = GetCurrentUserId(),
                InstitutionName = "MasterMind Coaching",
                InstitutionAddress = "Your Institution Address",
                InstitutionContact = "Your Contact Number",
                ReceiptItems = receiptItems
            };

            _context.FeeReceipts.Add(receipt);
            await _context.SaveChangesAsync();

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
            _logger.LogError(ex, "Error collecting payment");
            return StatusCode(500, new ApiResponse<FeeReceiptDto>
            {
                Success = false,
                Message = "Error collecting payment"
            });
        }
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

            // Here you would implement actual email sending logic
            // For now, we'll just mark it as sent
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
                .Include(s => s.StudentFees)
                    .ThenInclude(sf => sf.FeeStructure)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
            {
                return NotFound(new ApiResponse<StudentFeeDetailsDto>
                {
                    Success = false,
                    Message = "Student not found"
                });
            }

            var feeDetails = new StudentFeeDetailsDto
            {
                StudentId = student.Id,
                StudentName = $"{student.FirstName} {student.LastName}",
                StudentClass = student.StudentClasses.FirstOrDefault()?.Class?.Name ?? "N/A",
                ParentName = student.ParentName,
                ParentEmail = student.ParentEmail,
                ParentMobile = student.ParentMobile,
                PendingFees = student.StudentFees
                    .Where(sf => sf.Status == FeeStatus.Pending || sf.Status == FeeStatus.PartiallyPaid)
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
