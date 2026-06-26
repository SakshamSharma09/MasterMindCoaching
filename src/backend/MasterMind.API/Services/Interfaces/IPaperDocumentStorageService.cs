namespace MasterMind.API.Services.Interfaces;

public interface IPaperDocumentStorageService
{
    bool IsConfigured { get; }
    Task UploadAsync(string containerName, string blobName, Stream content, string contentType, CancellationToken cancellationToken = default);
    Task<Stream> DownloadAsync(string containerName, string blobName, CancellationToken cancellationToken = default);
    Task DeleteIfExistsAsync(string containerName, string blobName, CancellationToken cancellationToken = default);
}
