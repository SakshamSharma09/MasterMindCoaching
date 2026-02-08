using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MasterMind.API.Models.Entities
{
    public class UserDevice
    {
        public int Id { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }
        
        public User User { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string DeviceId { get; set; } = string.Empty; // Unique device identifier

        [Required]
        [MaxLength(100)]
        public string DeviceName { get; set; } = string.Empty; // e.g., "iPhone 13", "Chrome on Windows"

        [Required]
        [MaxLength(50)]
        public string DeviceType { get; set; } = string.Empty; // "Mobile", "Desktop", "Tablet"

        [Required]
        [MaxLength(100)]
        public string BrowserInfo { get; set; } = string.Empty; // User agent string

        [Required]
        [MaxLength(45)]
        public string IpAddress { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Location { get; set; } = string.Empty; // City, Country

        public bool IsTrusted { get; set; } = false; // User marks device as trusted

        public bool IsActive { get; set; } = true; // Device is currently active

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastUsedAt { get; set; }

        public DateTime? ExpiresAt { get; set; } // Device session expiry

        // Navigation properties for relationships
        public virtual ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();
    }

    public class UserSession
    {
        public int Id { get; set; }

        [Required]
        [ForeignKey("UserDevice")]
        public int UserDeviceId { get; set; }
        
        public UserDevice UserDevice { get; set; } = null!;

        [Required]
        public string Token { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime ExpiresAt { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? LastActivityAt { get; set; }
    }
}
