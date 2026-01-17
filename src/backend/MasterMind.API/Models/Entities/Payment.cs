namespace MasterMind.API.Models.Entities;

/// <summary>
/// Payment entity for tracking fee payments
/// </summary>
public class Payment : BaseEntity
{
    public int StudentFeeId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public PaymentMethod Method { get; set; }
    public string? TransactionId { get; set; }
    public string? ReceiptNumber { get; set; }
    public string? Remarks { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Completed;
    public int? ReceivedByUserId { get; set; }

    // Navigation properties
    public StudentFee StudentFee { get; set; } = null!;
    public User? ReceivedByUser { get; set; }
}

public enum PaymentMethod
{
    Cash,
    UPI,
    BankTransfer,
    CreditCard,
    DebitCard,
    Cheque,
    Online,
    Other
}

public enum PaymentStatus
{
    Pending,
    Completed,
    Failed,
    Refunded,
    Cancelled
}
