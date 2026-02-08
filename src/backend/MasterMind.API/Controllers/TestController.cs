using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MasterMind.API.Data;
using System.Data.Common;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly MasterMindDbContext _context;

    public TestController(MasterMindDbContext context)
    {
        _context = context;
    }

    [HttpGet("student-count")]
    public async Task<IActionResult> GetStudentCount()
    {
        try
        {
            // Try a simple count first
            var count = await _context.Students.CountAsync();
            return Ok(new { success = true, count = count });
        }
        catch (Exception ex)
        {
            return Ok(new { success = false, error = ex.Message });
        }
    }

    [HttpGet("student-columns")]
    public async Task<IActionResult> GetStudentColumns()
    {
        try
        {
            var columns = new List<string>();
            await using var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();
            
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Students' ORDER BY ORDINAL_POSITION";
            
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                columns.Add(reader.GetString(0));
            }
            
            return Ok(new { success = true, columns = columns });
        }
        catch (Exception ex)
        {
            return Ok(new { success = false, error = ex.Message });
        }
    }
}
