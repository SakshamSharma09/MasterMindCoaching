using MasterMind.API.Models.Entities;

namespace MasterMind.API.Services.Interfaces
{
    public interface IDeviceService
    {
        Task<UserDevice> RegisterDeviceAsync(int userId, string deviceId, string deviceName, string deviceType, string browserInfo, string ipAddress, string location);
        Task<UserDevice?> GetDeviceAsync(int userId, string deviceId);
        Task<List<UserDevice>> GetUserDevicesAsync(int userId);
        Task<bool> IsDeviceTrustedAsync(int userId, string deviceId);
        Task TrustDeviceAsync(int userId, string deviceId);
        Task UpdateDeviceActivityAsync(int userId, string deviceId);
        Task RevokeDeviceAsync(int userId, string deviceId);
        Task CleanupExpiredDevicesAsync();
    }
}
