using MasterMind.API.Data;
using MasterMind.API.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MasterMind.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class FeesController : ControllerBase
{
    private readonly MasterMindDbContext _context;
    private readonly ILogger<FeesController> _logger;

    public FeesController(MasterMindDbContext context, ILogger<FeesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all fees with optional filtering
    /// </summary>
    /// <param name="classId">Optional class ID filter</param>
    /// <param name="status">Optional status filter</param>
    /// <param name="month">Optional month filter</param>
    /// <returns>List of fees</returns>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<StudentFeeDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<StudentFeeDto>>>> GetFees(
        [FromQuery] int? classId,
        [FromQuery] string? status,
        [FromQuery] string? month)
    {
        try
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

            var fees = await query
                .OrderByDescending(sf => sf.DueDate)
                .Select(sf => new StudentFeeDto
                {
                    Id = sf.Id,
                    StudentId = sf.StudentId,
                    StudentName = $"{sf.Student.FirstName} {sf.Student.LastName}",
                    ClassId = sf.Student.StudentClasses.FirstOrDefault()!.ClassId,
                    ClassName = sf.Student.StudentClasses.FirstOrDefault()!.Class.Name,
                    FeeType = sf.FeeStructure.Name,
                    Amount = sf.FinalAmount,
                    DueDate = sf.DueDate.ToString("yyyy-MM-dd"),
                    Status = sf.Status.ToString(),
                    Description = sf.Remarks,
                    PaidAmount = sf.PaidAmount,
                    BalanceAmount = sf.BalanceAmount,
                    IsOverdue = sf.IsOverdue
                })
                .ToListAsync();

            return Ok(new ApiResponse<IEnumerable<StudentFeeDto>>
            {
                Success = true,
                Message = "Fees retrieved successfully",
                Data = fees
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving fees");
            return StatusCode(500, new ApiResponse<IEnumerable<StudentFeeDto>>
            {
                Success = false,
                Message = "Error retrieving fees"
            });
        }
    }

    /// <summary>
    /// Update an existing fee
    /// </summary>
    /// <param name="id">Fee ID</param>
    /// <param name="request">Fee update request</param>
    /// <returns>Updated fee</returns>
    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<StudentFeeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<StudentFeeDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<StudentFeeDto>>> UpdateFee(int id, [FromBody] UpdateFeeRequest request)
    {
        try
        {
            var studentFee = await _context.StudentFees
                .Include(sf => sf.Student)
                    .ThenInclude(s => s.StudentClasses)
                        .ThenInclude(sc => sc.Class)
                .Include(sf => sf.FeeStructure)
                .FirstOrDefaultAsync(sf => sf.Id == id);

            if (studentFee == null)
            {
                return NotFound(new ApiResponse<StudentFeeDto>
                {
                    Success = false,
                    Message = "Fee not found"
                });
            }

            // Don't allow updates if fee is already paid
            if (studentFee.Status == FeeStatus.Paid)
            {
                return BadRequest(new ApiResponse<StudentFeeDto>
                {
                    Success = false,
                    Message = "Cannot update a paid fee"
                });
            }

            if (!string.IsNullOrEmpty(request.DueDate))
            {
                studentFee.DueDate = DateOnly.Parse(request.DueDate);
            }

            if (!string.IsNullOrEmpty(request.Description))
            {
                studentFee.Remarks = request.Description;
            }

            if (request.DiscountAmount.HasValue)
            {
                studentFee.DiscountAmount = request.DiscountAmount.Value;
                studentFee.FinalAmount = studentFee.Amount - request.DiscountAmount.Value;
                // BalanceAmount is now computed automatically: FinalAmount - PaidAmount
            }

            await _context.SaveChangesAsync();

            var studentClass = studentFee.Student.StudentClasses.FirstOrDefault();
            var feeDto = new StudentFeeDto
            {
                Id = studentFee.Id,
                StudentId = studentFee.StudentId,
                StudentName = $"{studentFee.Student.FirstName} {studentFee.Student.LastName}",
                ClassId = studentClass?.ClassId ?? 0,
                ClassName = studentClass?.Class.Name ?? "N/A",
                FeeType = studentFee.FeeStructure.Name,
                Amount = studentFee.FinalAmount,
                DueDate = studentFee.DueDate.ToString("yyyy-MM-dd"),
                Status = studentFee.Status.ToString(),
                Description = studentFee.Remarks,
                PaidAmount = studentFee.PaidAmount,
                BalanceAmount = studentFee.BalanceAmount,
                IsOverdue = studentFee.IsOverdue
            };

            return Ok(new ApiResponse<StudentFeeDto>
            {
                Success = true,
                Message = "Fee updated successfully",
                Data = feeDto
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating fee");
            return StatusCode(500, new ApiResponse<StudentFeeDto>
            {
                Success = false,
                Message = "Error updating fee"
            });
        }
    }

    /// <summary>
    /// Delete a fee
    /// </summary>
    /// <param name="id">Fee ID</param>
    /// <returns>Success response</returns>
    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteFee(int id)
    {
        try
        {
            var studentFee = await _context.StudentFees
                .Include(sf => sf.Payments)
                .FirstOrDefaultAsync(sf => sf.Id == id);

            if (studentFee == null)
            {
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Fee not found"
                });
            }

            // Don't allow deletion if there are payments
            if (studentFee.Payments.Any())
            {
                return BadRequest(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Cannot delete fee with existing payments"
                });
            }

            _context.StudentFees.Remove(studentFee);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = "Fee deleted successfully",
                Data = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting fee");
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = "Error deleting fee"
            });
        }
    }

    /// <summary>
    /// Get overdue fees
    /// </summary>
    /// <returns>List of overdue fees</returns>
    [HttpGet("overdue")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<OverdueFeeDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<OverdueFeeDto>>>> GetOverdueFees()
    {
        try
        {
            var overdueFees = await _context.StudentFees
                .Include(sf => sf.Student)
                    .ThenInclude(s => s.StudentClasses)
                        .ThenInclude(sc => sc.Class)
                .Include(sf => sf.FeeStructure)
                .Where(sf => sf.Status != FeeStatus.Paid && DateOnly.FromDateTime(DateTime.Today) > sf.DueDate)
                .Select(sf => new OverdueFeeDto
                {
                    Id = sf.Id,
                    StudentId = sf.StudentId,
                    StudentName = $"{sf.Student.FirstName} {sf.Student.LastName}",
                    ClassId = sf.Student.StudentClasses.FirstOrDefault()!.ClassId,
                    ClassName = sf.Student.StudentClasses.FirstOrDefault()!.Class.Name,
                    FeeType = sf.FeeStructure.Name,
                    Amount = sf.FinalAmount,
                    DueDate = sf.DueDate.ToString("yyyy-MM-dd"),
                    Status = sf.Status.ToString(),
                    Description = sf.Remarks,
                    DaysOverdue = DateOnly.FromDateTime(DateTime.Today).DayNumber - sf.DueDate.DayNumber,
                    ParentContact = sf.Student.ParentMobile,
                    BalanceAmount = sf.BalanceAmount
                })
                .ToListAsync();

            return Ok(new ApiResponse<IEnumerable<OverdueFeeDto>>
            {
                Success = true,
                Message = "Overdue fees retrieved successfully",
                Data = overdueFees
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving overdue fees");
            return StatusCode(500, new ApiResponse<IEnumerable<OverdueFeeDto>>
            {
                Success = false,
                Message = "Error retrieving overdue fees"
            });
        }
    }

    /// <summary>
    /// Send reminders for overdue fees
    /// </summary>
    /// <param name="request">Reminder request</param>
    /// <returns>Success response</returns>
    [HttpPost("reminders")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> SendReminders([FromBody] SendRemindersRequest request)
    {
        try
        {
            var query = _context.StudentFees
                .Include(sf => sf.Student)
                .Where(sf => sf.Status != FeeStatus.Paid && DateOnly.FromDateTime(DateTime.Today) > sf.DueDate);

            if (request.FeeIds != null && request.FeeIds.Any())
            {
                query = query.Where(sf => request.FeeIds.Contains(sf.Id));
            }

            var overdueFees = await query.ToListAsync();

            // Here you would implement actual SMS/Email sending logic
            // For now, we'll just log it
            foreach (var fee in overdueFees)
            {
                _logger.LogInformation($"Reminder sent to {fee.Student.ParentName} ({fee.Student.ParentMobile}) for fee {fee.Id}");
            }

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = $"Reminders sent to {overdueFees.Count} parents",
                Data = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending reminders");
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = "Error sending reminders"
            });
        }
    }

    /// <summary>
    /// Mark fee as paid
    /// </summary>
    /// <param name="id">Fee ID</param>
    /// <returns>Updated fee</returns>
    [HttpPost("{id}/mark-paid")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<StudentFeeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<StudentFeeDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<StudentFeeDto>>> MarkFeeAsPaid(int id)
    {
        try
        {
            var studentFee = await _context.StudentFees
                .Include(sf => sf.Student)
                    .ThenInclude(s => s.StudentClasses)
                        .ThenInclude(sc => sc.Class)
                .Include(sf => sf.FeeStructure)
                .FirstOrDefaultAsync(sf => sf.Id == id);

            if (studentFee == null)
            {
                return NotFound(new ApiResponse<StudentFeeDto>
                {
                    Success = false,
                    Message = "Fee not found"
                });
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
                    ReceivedByUserId = GetCurrentUserId(),
                    Remarks = "Marked as paid manually"
                };

                _context.Payments.Add(payment);
            }

            studentFee.Status = FeeStatus.Paid;
            studentFee.PaidAmount = studentFee.FinalAmount;

            await _context.SaveChangesAsync();

            var studentClass = studentFee.Student.StudentClasses.FirstOrDefault();
            var feeDto = new StudentFeeDto
            {
                Id = studentFee.Id,
                StudentId = studentFee.StudentId,
                StudentName = $"{studentFee.Student.FirstName} {studentFee.Student.LastName}",
                ClassId = studentClass?.ClassId ?? 0,
                ClassName = studentClass?.Class.Name ?? "N/A",
                FeeType = studentFee.FeeStructure.Name,
                Amount = studentFee.FinalAmount,
                DueDate = studentFee.DueDate.ToString("yyyy-MM-dd"),
                Status = studentFee.Status.ToString(),
                Description = studentFee.Remarks,
                PaidAmount = studentFee.PaidAmount,
                BalanceAmount = studentFee.BalanceAmount,
                IsOverdue = studentFee.IsOverdue
            };

            return Ok(new ApiResponse<StudentFeeDto>
            {
                Success = true,
                Message = "Fee marked as paid successfully",
                Data = feeDto
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking fee as paid");
            return StatusCode(500, new ApiResponse<StudentFeeDto>
            {
                Success = false,
                Message = "Error marking fee as paid"
            });
        }
    }

    /// <summary>
    /// Get fee structures
    /// </summary>
    /// <returns>List of fee structures</returns>
    [HttpGet("structures")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<FeeStructureDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<FeeStructureDto>>>> GetFeeStructures()
    {
        try
        {
            var feeStructures = await _context.FeeStructures
                .Include(fs => fs.Class)
                .Where(fs => fs.IsActive)
                .OrderBy(fs => fs.Name)
                .Select(fs => new FeeStructureDto
                {
                    Id = fs.Id,
                    Name = fs.Name,
                    Type = fs.Type.ToString(),
                    Amount = fs.Amount,
                    Frequency = fs.Frequency.ToString(),
                    ClassId = fs.ClassId,
                    ClassName = fs.Class != null ? fs.Class.Name : null,
                    Description = fs.Description,
                    AcademicYear = fs.AcademicYear
                })
                .ToListAsync();

            return Ok(new ApiResponse<IEnumerable<FeeStructureDto>>
            {
                Success = true,
                Message = "Fee structures retrieved successfully",
                Data = feeStructures
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving fee structures");
            return StatusCode(500, new ApiResponse<IEnumerable<FeeStructureDto>>
            {
                Success = false,
                Message = "Error retrieving fee structures"
            });
        }
    }

    private string GetCurrentAcademicYear()
    {
        // Simple logic - in real implementation, this would be more sophisticated
        var currentYear = DateTime.Now.Year;
        var nextYear = currentYear + 1;
        return $"{currentYear}-{nextYear}";
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}

// DTOs
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

public class UpdateFeeRequest
{
    public string? DueDate { get; set; }
    public decimal? DiscountAmount { get; set; }
    public string? Description { get; set; }
}

public class SendRemindersRequest
{
    public List<int>? FeeIds { get; set; }
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
