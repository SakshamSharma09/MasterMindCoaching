using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MasterMind.API.Services.Interfaces;

namespace MasterMind.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class PhotoController : ControllerBase
{
    private readonly IBlobStorageService _blobStorageService;
    private readonly ILogger<PhotoController> _logger;
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

    public PhotoController(IBlobStorageService blobStorageService, ILogger<PhotoController> logger)
    {
        _blobStorageService = blobStorageService;
        _logger = logger;
    }

    [HttpPost("upload")]
    [RequestSizeLimit(MaxFileSize)]
    public async Task<IActionResult> UploadPhoto(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { success = false, message = "No file uploaded" });
        }

        if (file.Length > MaxFileSize)
        {
            return BadRequest(new { success = false, message = "File size exceeds 5MB limit" });
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
        {
            return BadRequest(new { success = false, message = "Invalid file type. Allowed: jpg, jpeg, png, gif, webp" });
        }

        try
        {
            using var stream = file.OpenReadStream();
            var blobName = await _blobStorageService.UploadPhotoAsync(stream, file.FileName, file.ContentType);
            var photoUrl = _blobStorageService.GetPhotoUrl(blobName);

            return Ok(new
            {
                success = true,
                message = "Photo uploaded successfully",
                data = new
                {
                    blobName,
                    url = photoUrl
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload photo");
            return StatusCode(500, new { success = false, message = "Failed to upload photo" });
        }
    }

    [HttpGet("{blobName}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPhoto(string blobName)
    {
        try
        {
            var stream = await _blobStorageService.DownloadPhotoAsync(blobName);
            
            if (stream == null)
            {
                return NotFound(new { success = false, message = "Photo not found" });
            }

            var extension = Path.GetExtension(blobName).ToLowerInvariant();
            var contentType = extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };

            return File(stream, contentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get photo: {BlobName}", blobName);
            return StatusCode(500, new { success = false, message = "Failed to retrieve photo" });
        }
    }

    [HttpDelete("{blobName}")]
    public async Task<IActionResult> DeletePhoto(string blobName)
    {
        try
        {
            var deleted = await _blobStorageService.DeletePhotoAsync(blobName);
            
            if (!deleted)
            {
                return NotFound(new { success = false, message = "Photo not found" });
            }

            return Ok(new { success = true, message = "Photo deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete photo: {BlobName}", blobName);
            return StatusCode(500, new { success = false, message = "Failed to delete photo" });
        }
    }

    [HttpGet("url/{blobName}")]
    [AllowAnonymous]
    public IActionResult GetPhotoUrl(string blobName)
    {
        var url = _blobStorageService.GetPhotoUrl(blobName);
        
        if (string.IsNullOrEmpty(url))
        {
            return NotFound(new { success = false, message = "Photo not found" });
        }

        return Ok(new { success = true, url });
    }
}
