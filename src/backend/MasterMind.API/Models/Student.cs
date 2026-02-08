using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MasterMind.API.Models
{
    public class Student
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = "";

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = "";

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        [StringLength(255)]
        [EmailAddress]
        public string StudentEmail { get; set; } = "";

        [StringLength(20)]
        public string StudentMobile { get; set; } = "";

        [StringLength(20)]
        public string ParentMobile { get; set; } = "";

        [StringLength(200)]
        public string ParentName { get; set; } = "";

        [StringLength(500)]
        public string Address { get; set; } = "";

        [StringLength(100)]
        public string City { get; set; } = "";

        [StringLength(100)]
        public string State { get; set; } = "";

        [StringLength(20)]
        public string PinCode { get; set; } = "";

        [StringLength(500)]
        public string ProfileImageUrl { get; set; } = "";

        [StringLength(12)]
        public string AadharNumber { get; set; } = "";

        [StringLength(50)]
        public string RollNumber { get; set; } = "";

        [StringLength(50)]
        public string Standard { get; set; } = "";

        [StringLength(50)]
        public string Cast { get; set; } = "";

        [StringLength(10)]
        public string Gender { get; set; } = "";

        [StringLength(200)]
        public string CurrentSchool { get; set; } = "";

        [StringLength(200)]
        public string AdmissionNumber { get; set; } = "";

        [Column(TypeName = "date")]
        public DateTime? AdmissionDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DateOfBirth { get; set; }

        [StringLength(255)]
        public string ParentEmail { get; set; } = "";

        [StringLength(200)]
        public string ParentOccupation { get; set; } = "";

        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<StudentClass> StudentClasses { get; set; } = new List<StudentClass>();
    }
}
