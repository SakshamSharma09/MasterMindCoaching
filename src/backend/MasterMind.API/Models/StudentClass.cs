using System.ComponentModel.DataAnnotations;

namespace MasterMind.API.Models
{
    public class StudentClass
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int ClassId { get; set; }
        
        [Required]
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public Student Student { get; set; }
        public Class Class { get; set; }
    }
}
