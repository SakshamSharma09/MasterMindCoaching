namespace MasterMind.API.Services.Interfaces;

public interface IBlobStorageService
{
    Task<string> UploadPhotoAsync(Stream fileStream, string fileName, string contentType);
    Task<Stream?> DownloadPhotoAsync(string blobName);
    Task<bool> DeletePhotoAsync(string blobName);
    string GetPhotoUrl(string blobName);
}
