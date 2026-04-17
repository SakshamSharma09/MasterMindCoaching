using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using MasterMind.API.Services.Interfaces;

namespace MasterMind.API.Services.Implementations;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _containerClient;
    private readonly ILogger<BlobStorageService> _logger;
    private readonly string _containerName = "student-photos";

    public BlobStorageService(IConfiguration configuration, ILogger<BlobStorageService> logger)
    {
        _logger = logger;
        var connectionString = configuration["AzureBlobStorage:ConnectionString"];
        
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Azure Blob Storage connection string is not configured.");
        }

        var blobServiceClient = new BlobServiceClient(connectionString);
        _containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
        _containerClient.CreateIfNotExists(PublicAccessType.Blob);
    }

    public async Task<string> UploadPhotoAsync(Stream fileStream, string fileName, string contentType)
    {
        try
        {
            var blobName = $"{Guid.NewGuid()}-{SanitizeFileName(fileName)}";
            var blobClient = _containerClient.GetBlobClient(blobName);

            var blobHttpHeaders = new BlobHttpHeaders
            {
                ContentType = contentType
            };

            await blobClient.UploadAsync(fileStream, new BlobUploadOptions
            {
                HttpHeaders = blobHttpHeaders
            });

            _logger.LogInformation("Uploaded photo: {BlobName}", blobName);
            return blobName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload photo: {FileName}", fileName);
            throw;
        }
    }

    public async Task<Stream?> DownloadPhotoAsync(string blobName)
    {
        try
        {
            var blobClient = _containerClient.GetBlobClient(blobName);
            
            if (!await blobClient.ExistsAsync())
            {
                return null;
            }

            var response = await blobClient.DownloadStreamingAsync();
            return response.Value.Content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download photo: {BlobName}", blobName);
            throw;
        }
    }

    public async Task<bool> DeletePhotoAsync(string blobName)
    {
        try
        {
            var blobClient = _containerClient.GetBlobClient(blobName);
            var response = await blobClient.DeleteIfExistsAsync();
            
            _logger.LogInformation("Deleted photo: {BlobName}, Success: {Success}", blobName, response.Value);
            return response.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete photo: {BlobName}", blobName);
            throw;
        }
    }

    public string GetPhotoUrl(string blobName)
    {
        if (string.IsNullOrEmpty(blobName))
        {
            return string.Empty;
        }

        var blobClient = _containerClient.GetBlobClient(blobName);
        return blobClient.Uri.ToString();
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        return sanitized.ToLowerInvariant();
    }
}
