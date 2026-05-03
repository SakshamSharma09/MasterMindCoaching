namespace MasterMind.API.Models.Entities;

public class TemplateDispatchLog : BaseEntity
{
    public int MessageTemplateId { get; set; }
    public int? StudentId { get; set; }
    public int? StudentFeeId { get; set; }
    public int? FeeReceiptId { get; set; }
    public string Channel { get; set; } = "System";
    public string Status { get; set; } = "Generated";
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SentAt { get; set; }
    public string RenderedSubject { get; set; } = string.Empty;
    public string RenderedBody { get; set; } = string.Empty;

    public MessageTemplate MessageTemplate { get; set; } = null!;
    public Student? Student { get; set; }
    public StudentFee? StudentFee { get; set; }
    public FeeReceipt? FeeReceipt { get; set; }
}
