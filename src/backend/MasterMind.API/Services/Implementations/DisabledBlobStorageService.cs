using MasterMind.API.Services.Interfaces;

namespace MasterMind.API.Services.Implementations;

public class DisabledBlobStorageService : IBlobStorageService
{
    private readonly ILogger<DisabledBlobStorageService> _logger;

    public DisabledBlobStorageService(ILogger<DisabledBlobStorageService> logger)
    {
        _logger = logger;
    }

    public Task<string> UploadPhotoAsync(Stream fileStream, string fileName, string contentType)
    {
        _logger.LogWarning("Photo upload attempted, but Azure Blob Storage is not configured.");
        throw new InvalidOperationException("Photo uploads are disabled because Azure Blob Storage is not configured.");
    }

    public Task<Stream?> DownloadPhotoAsync(string blobName)
    {
        _logger.LogDebug("Photo download requested while Azure Blob Storage is not configured: {BlobName}", blobName);
        return Task.FromResult<Stream?>(null);
    }

    public Task<bool> DeletePhotoAsync(string blobName)
    {
        _logger.LogDebug("Photo delete requested while Azure Blob Storage is not configured: {BlobName}", blobName);
        return Task.FromResult(false);
    }

    public string GetPhotoUrl(string blobName)
    {
        return string.Empty;
    }
}
