namespace MasterMind.API.Models.Entities;

public class MessageTemplate : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public TemplateType Type { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int AutoReminderDaysBefore { get; set; } = 0;
    public string? Frequency { get; set; }
    public string? VariablesJson { get; set; }
}

public enum TemplateType
{
    BirthdayWish = 1,
    FeeReminder = 2,
    FeeReceipt = 3
}
