using MasterMind.API.Data;
using MasterMind.API.Models.Entities;
using MasterMind.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace MasterMind.API.Services.Implementations
{
    public class DeviceService : IDeviceService
    {
        private readonly MasterMindDbContext _context;
        private readonly ILogger<DeviceService> _logger;

        public DeviceService(MasterMindDbContext context, ILogger<DeviceService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<UserDevice> RegisterDeviceAsync(int userId, string deviceId, string deviceName, string deviceType, string browserInfo, string ipAddress, string location)
        {
            try
            {
                // Check if device already exists
                var existingDevice = await _context.UserDevices
                    .FirstOrDefaultAsync(d => d.UserId == userId && d.DeviceId == deviceId);

                if (existingDevice == null)
                {
                    // Create new device
                    var device = new UserDevice
                    {
                        UserId = userId,
                        DeviceId = deviceId,
                        DeviceName = deviceName,
                        DeviceType = deviceType,
                        BrowserInfo = browserInfo,
                        IpAddress = ipAddress,
                        Location = location,
                        IsTrusted = false,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        LastUsedAt = DateTime.UtcNow,
                        ExpiresAt = DateTime.UtcNow.AddDays(30) // Device expires in 30 days
                    };

                    _context.UserDevices.Add(device);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("New device registered for user {UserId}: {DeviceName} ({DeviceType})", userId, deviceName, deviceType);
                    return device;
                }
                else
                {
                    // Update existing device
                    existingDevice.LastUsedAt = DateTime.UtcNow;
                    existingDevice.IsActive = true;
                    existingDevice.ExpiresAt = DateTime.UtcNow.AddDays(30);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Existing device updated for user {UserId}: {DeviceName}", userId, deviceName);
                    return existingDevice;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering device for user {UserId}", userId);
                throw;
            }
        }

        public async Task<UserDevice?> GetDeviceAsync(int userId, string deviceId)
        {
            return await _context.UserDevices
                .Include(d => d.Sessions)
                .FirstOrDefaultAsync(d => d.UserId == userId && d.DeviceId == deviceId && d.IsActive);
        }

        public async Task<List<UserDevice>> GetUserDevicesAsync(int userId)
        {
            return await _context.UserDevices
                .Where(d => d.UserId == userId && d.IsActive)
                .OrderByDescending(d => d.LastUsedAt)
                .ToListAsync();
        }

        public async Task<bool> IsDeviceTrustedAsync(int userId, string deviceId)
        {
            var device = await GetDeviceAsync(userId, deviceId);
            return device?.IsTrusted ?? false;
        }

        public async Task TrustDeviceAsync(int userId, string deviceId)
        {
            var device = await GetDeviceAsync(userId, deviceId);
            if (device != null)
            {
                device.IsTrusted = true;
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Device {DeviceId} marked as trusted for user {UserId}", deviceId, userId);
            }
        }

        public async Task UpdateDeviceActivityAsync(int userId, string deviceId)
        {
            var device = await GetDeviceAsync(userId, deviceId);
            if (device != null)
            {
                device.LastUsedAt = DateTime.UtcNow;
                device.ExpiresAt = DateTime.UtcNow.AddDays(30);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RevokeDeviceAsync(int userId, string deviceId)
        {
            var device = await GetDeviceAsync(userId, deviceId);
            if (device != null)
            {
                device.IsActive = false;
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Device {DeviceId} revoked for user {UserId}", deviceId, userId);
            }
        }

        public async Task CleanupExpiredDevicesAsync()
        {
            var expiredDevices = await _context.UserDevices
                .Where(d => d.ExpiresAt < DateTime.UtcNow && d.IsActive)
                .ToListAsync();

            foreach (var device in expiredDevices)
            {
                device.IsActive = false;
            }

            if (expiredDevices.Any())
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Cleaned up {Count} expired devices", expiredDevices.Count);
            }
        }

        public static string GenerateDeviceId()
        {
            return Guid.NewGuid().ToString("N")[..8]; // Short device ID
        }

        public static string GetDeviceType(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
                return "Unknown";

            var ua = userAgent.ToLower();
            
            if (ua.Contains("mobile") || ua.Contains("android") || ua.Contains("iphone"))
                return "Mobile";
            else if (ua.Contains("tablet") || ua.Contains("ipad"))
                return "Tablet";
            else
                return "Desktop";
        }

        public static string GetDeviceName(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
                return "Unknown Device";

            var ua = userAgent.ToLower();
            
            if (ua.Contains("iphone"))
                return "iPhone";
            else if (ua.Contains("ipad"))
                return "iPad";
            else if (ua.Contains("android"))
                return "Android Device";
            else if (ua.Contains("chrome"))
                return "Chrome";
            else if (ua.Contains("firefox"))
                return "Firefox";
            else if (ua.Contains("safari"))
                return "Safari";
            else if (ua.Contains("edge"))
                return "Edge";
            else
                return "Unknown Device";
        }

        public static string GetLocationFromIp(string ipAddress)
        {
            // This is a simplified implementation
            // In production, you'd use a proper IP geolocation service
            if (ipAddress == "127.0.0.1" || ipAddress == "::1")
                return "Localhost";
            
            return "Unknown Location";
        }
    }
}
