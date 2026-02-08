using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MasterMind.API.Models
{
    public class Class
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(50)]
        public string Medium { get; set; }

        [StringLength(100)]
        public string Board { get; set; }

        [StringLength(20)]
        public string AcademicYear { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Active";

        public int MaxStudents { get; set; } = 30;

        public int StudentCount { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<StudentClass> StudentClasses { get; set; }
        
        [NotMapped]
        public List<string> Subjects { get; set; } = new List<string>();
        
        [NotMapped]
        public List<string> Teachers { get; set; } = new List<string>();
    }
}
