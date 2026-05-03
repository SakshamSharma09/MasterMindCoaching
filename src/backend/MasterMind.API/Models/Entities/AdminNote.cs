namespace MasterMind.API.Models.Entities;

public class AdminNote : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime NoteDate { get; set; } = DateTime.UtcNow.Date;
}
