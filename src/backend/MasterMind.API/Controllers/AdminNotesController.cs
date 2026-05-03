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
}
