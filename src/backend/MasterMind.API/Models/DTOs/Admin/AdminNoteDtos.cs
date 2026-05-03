using System.ComponentModel.DataAnnotations;

namespace MasterMind.API.Models.DTOs.Admin;

public class CreateAdminNoteRequest
{
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(4000, MinimumLength = 2)]
    public string Content { get; set; } = string.Empty;

    [Required]
    public DateTime NoteDate { get; set; }
}

public class UpdateAdminNoteRequest
{
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(4000, MinimumLength = 2)]
    public string Content { get; set; } = string.Empty;

    [Required]
    public DateTime NoteDate { get; set; }
}
