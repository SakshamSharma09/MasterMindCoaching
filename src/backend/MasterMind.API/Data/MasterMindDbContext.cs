using Microsoft.EntityFrameworkCore;
using MasterMind.API.Models.Entities;

namespace MasterMind.API.Data
{
    public class MasterMindDbContext : DbContext
    {
        public MasterMindDbContext(DbContextOptions<MasterMindDbContext> options) : base(options)
        {
        }

        // Student Management
        public DbSet<MasterMind.API.Models.Entities.Student> Students { get; set; }
        public DbSet<MasterMind.API.Models.Entities.Class> Classes { get; set; }
        public DbSet<MasterMind.API.Models.Entities.StudentClass> StudentClasses { get; set; }
        public DbSet<MasterMind.API.Models.Entities.Teacher> Teachers { get; set; }
        public DbSet<MasterMind.API.Models.Entities.Subject> Subjects { get; set; }

        // User Management
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<UserDevice> UserDevices { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<OtpRecord> OtpRecords { get; set; }

        // Attendance Management
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<TeacherAttendance> TeacherAttendances { get; set; }

        // Fee Management
        public DbSet<Payment> Payments { get; set; }
        public DbSet<FeeStructure> FeeStructures { get; set; }
        public DbSet<StudentFee> StudentFees { get; set; }
        public DbSet<FeeReceipt> FeeReceipts { get; set; }
        public DbSet<FeeReceiptItem> FeeReceiptItems { get; set; }
        public DbSet<FeePaymentSchedule> FeePaymentSchedules { get; set; }
        public DbSet<FeeInstallment> FeeInstallments { get; set; }

        // Expense Management
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<BudgetCategory> BudgetCategories { get; set; }

        // Lead Management
        public DbSet<Lead> Leads { get; set; }
        public DbSet<LeadFollowup> LeadFollowups { get; set; }

        // Performance Management
        public DbSet<StudentPerformance> StudentPerformances { get; set; }
        public DbSet<StudentRemark> StudentRemarks { get; set; }

        // Teacher Management
        public DbSet<TeacherClass> TeacherClasses { get; set; }
        public DbSet<TeacherSalary> TeacherSalaries { get; set; }

        // Class-Subject Relationship
        public DbSet<ClassSubject> ClassSubjects { get; set; }

        // Session Management
        public DbSet<Session> Sessions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // UserRole Configuration (Many-to-Many)
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(ur => new { ur.UserId, ur.RoleId });

                entity.HasOne(ur => ur.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Student configuration
            modelBuilder.Entity<MasterMind.API.Models.Entities.Student>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.StudentEmail).HasMaxLength(255);
                entity.Property(e => e.StudentMobile).HasMaxLength(20);
                entity.Property(e => e.ParentMobile).HasMaxLength(20);
                entity.Property(e => e.Address).HasMaxLength(500);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.State).HasMaxLength(100);
                entity.Property(e => e.PinCode).HasMaxLength(20);
                entity.Property(e => e.ProfileImageUrl).HasMaxLength(500);
                entity.Property(e => e.AdmissionNumber).HasMaxLength(200);
                entity.Property(e => e.ParentName).HasMaxLength(200);
                entity.Property(e => e.ParentEmail).HasMaxLength(255);
                entity.Property(e => e.ParentOccupation).HasMaxLength(200);
                entity.Property(e => e.DateOfBirth).HasColumnType("date");
                entity.Property(e => e.AdmissionDate).HasColumnType("date");

                // Configure Session relationship
                entity.HasOne(s => s.Session)
                    .WithMany(session => session.Students)
                    .HasForeignKey(s => s.SessionId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Class configuration
            modelBuilder.Entity<MasterMind.API.Models.Entities.Class>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Medium).HasMaxLength(50);
                entity.Property(e => e.Board).HasMaxLength(100);
                entity.Property(e => e.AcademicYear).HasMaxLength(20);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
            });

            // StudentClass configuration
            modelBuilder.Entity<MasterMind.API.Models.Entities.StudentClass>(entity =>
            {
                entity.HasKey(sc => new { sc.StudentId, sc.ClassId });
                entity.Property(e => e.StudentId).IsRequired();
                entity.Property(e => e.ClassId).IsRequired();
                entity.Property(e => e.IsActive).IsRequired();
                entity.Property(e => e.EnrollmentDate).HasDefaultValueSql("GETUTCDATE()");

                // Configure relationships
                entity.HasOne(d => d.Student)
                    .WithMany(p => p.StudentClasses)
                    .HasForeignKey(d => d.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Class)
                    .WithMany(p => p.StudentClasses)
                    .HasForeignKey(d => d.ClassId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Teacher configuration
            modelBuilder.Entity<MasterMind.API.Models.Entities.Teacher>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Mobile).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Specialization).HasMaxLength(200);
                entity.Property(e => e.Qualification).HasMaxLength(500);
                entity.Property(e => e.Subjects).HasMaxLength(1000);
                entity.Property(e => e.JoiningDate).HasColumnType("date");
            });

            // Subject configuration
            modelBuilder.Entity<MasterMind.API.Models.Entities.Subject>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Code).HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(1000);
            });

            // ClassSubject configuration
            modelBuilder.Entity<ClassSubject>(entity =>
            {
                entity.HasKey(cs => new { cs.ClassId, cs.SubjectId });
                entity.Property(e => e.TeacherAssigned).HasMaxLength(200);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                // Configure relationships
                entity.HasOne(d => d.Class)
                    .WithMany(p => p.ClassSubjects)
                    .HasForeignKey(d => d.ClassId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Subject)
                    .WithMany(p => p.ClassSubjects)
                    .HasForeignKey(d => d.SubjectId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // TeacherClass configuration (Many-to-Many)
            modelBuilder.Entity<TeacherClass>(entity =>
            {
                entity.HasKey(tc => new { tc.TeacherId, tc.ClassId });

                entity.HasOne(tc => tc.Teacher)
                    .WithMany(t => t.TeacherClasses)
                    .HasForeignKey(tc => tc.TeacherId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(tc => tc.Class)
                    .WithMany(c => c.TeacherClasses)
                    .HasForeignKey(tc => tc.ClassId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
