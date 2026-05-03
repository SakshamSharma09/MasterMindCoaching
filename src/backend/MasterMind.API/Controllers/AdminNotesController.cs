using MasterMind.API.Data;
using MasterMind.API.Models.DTOs.Admin;
using MasterMind.API.Models.DTOs.Common;
using MasterMind.API.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MasterMind.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class AdminNotesController : ControllerBase
{
    private readonly MasterMindDbContext _context;

    public AdminNotesController(MasterMindDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<AdminNote>>>> GetNotes()
    {
        await EnsureAdminNotesTableAsync();

        var notes = await _context.AdminNotes
            .Where(n => !n.IsDeleted)
            .OrderByDescending(n => n.NoteDate)
            .ThenByDescending(n => n.CreatedAt)
            .ToListAsync();

        return Ok(new ApiResponse<IEnumerable<AdminNote>>
        {
            Success = true,
            Message = "Notes retrieved successfully",
            Data = notes
        });
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<AdminNote>>> Create([FromBody] CreateAdminNoteRequest request)
    {
        await EnsureAdminNotesTableAsync();

        var note = new AdminNote
        {
            Title = request.Title.Trim(),
            Content = request.Content.Trim(),
            NoteDate = request.NoteDate.Date
        };

        note.Id = 0;
        note.CreatedAt = DateTime.UtcNow;
        note.UpdatedAt = null;
        note.IsDeleted = false;

        _context.AdminNotes.Add(note);
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<AdminNote> { Success = true, Message = "Note created successfully", Data = note });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<AdminNote>>> Update(int id, [FromBody] UpdateAdminNoteRequest request)
    {
        await EnsureAdminNotesTableAsync();

        var note = await _context.AdminNotes.FirstOrDefaultAsync(n => n.Id == id && !n.IsDeleted);
        if (note == null)
        {
            return NotFound(new ApiResponse<AdminNote> { Success = false, Message = "Note not found", Data = null });
        }

        note.Title = request.Title.Trim();
        note.Content = request.Content.Trim();
        note.NoteDate = request.NoteDate.Date;
        note.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<AdminNote> { Success = true, Message = "Note updated successfully", Data = note });
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        await EnsureAdminNotesTableAsync();

        var note = await _context.AdminNotes.FirstOrDefaultAsync(n => n.Id == id && !n.IsDeleted);
        if (note == null)
        {
            return NotFound(new ApiResponse<bool> { Success = false, Message = "Note not found", Data = false });
        }

        note.IsDeleted = true;
        note.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(new ApiResponse<bool> { Success = true, Message = "Note deleted successfully", Data = true });
    }

    private async Task EnsureAdminNotesTableAsync()
    {
        if (!_context.Database.IsSqlServer())
        {
            return;
        }

        await _context.Database.ExecuteSqlRawAsync(@"
IF OBJECT_ID('dbo.AdminNotes', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AdminNotes
    (
        Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Title nvarchar(200) NOT NULL,
        Content nvarchar(4000) NOT NULL,
        NoteDate date NOT NULL,
        CreatedAt datetime2 NOT NULL DEFAULT(sysutcdatetime()),
        UpdatedAt datetime2 NULL,
        IsDeleted bit NOT NULL DEFAULT(0)
    );
END
");
    }
}
