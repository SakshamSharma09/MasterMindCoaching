using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using MasterMind.API.Services.Interfaces;

namespace MasterMind.API.Services.Implementations;

public class PaperDocumentStorageService : IPaperDocumentStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<PaperDocumentStorageService> _logger;

    public PaperDocumentStorageService(IConfiguration configuration, ILogger<PaperDocumentStorageService> logger)
    {
        var connectionString = configuration["AzureBlobStorage:ConnectionString"]
            ?? configuration["AzureBlobStorageConnectionString"]
            ?? configuration.GetConnectionString("AzureBlobStorage")
            ?? throw new InvalidOperationException("Azure Blob Storage connection string is not configured.");

        _blobServiceClient = new BlobServiceClient(connectionString);
        _logger = logger;
    }

    public bool IsConfigured => true;

    public async Task UploadAsync(string containerName, string blobName, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        var container = _blobServiceClient.GetBlobContainerClient(containerName);
        await container.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);

        var blob = container.GetBlobClient(blobName);
        content.Position = 0;

        await blob.UploadAsync(
            content,
            new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
            },
            cancellationToken);
    }

    public async Task<Stream> DownloadAsync(string containerName, string blobName, CancellationToken cancellationToken = default)
    {
        var blob = _blobServiceClient.GetBlobContainerClient(containerName).GetBlobClient(blobName);
        var response = await blob.DownloadStreamingAsync(cancellationToken: cancellationToken);
        return response.Value.Content;
    }

    public async Task DeleteIfExistsAsync(string containerName, string blobName, CancellationToken cancellationToken = default)
    {
        try
        {
            var blob = _blobServiceClient.GetBlobContainerClient(containerName).GetBlobClient(blobName);
            await blob.DeleteIfExistsAsync(cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to delete paper blob {Container}/{BlobName}", containerName, blobName);
        }
    }
}

public class DisabledPaperDocumentStorageService : IPaperDocumentStorageService
{
    public bool IsConfigured => false;

    public Task UploadAsync(string containerName, string blobName, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("Azure Blob Storage is not configured for Paper Generator.");
    }

    public Task<Stream> DownloadAsync(string containerName, string blobName, CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("Azure Blob Storage is not configured for Paper Generator.");
    }

    public Task DeleteIfExistsAsync(string containerName, string blobName, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
