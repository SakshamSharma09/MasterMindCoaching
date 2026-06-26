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

        // Exam Management
        public DbSet<Exam> Exams { get; set; }
        public DbSet<ExamResult> ExamResults { get; set; }
        public DbSet<AcademicPlannerEntry> AcademicPlannerEntries { get; set; }
        public DbSet<MessageTemplate> MessageTemplates { get; set; }
        public DbSet<TemplateDispatchLog> TemplateDispatchLogs { get; set; }
        public DbSet<AdminNote> AdminNotes { get; set; }
        public DbSet<PaperDocument> PaperDocuments { get; set; }
        public DbSet<PaperExtractedQuestion> PaperExtractedQuestions { get; set; }
        public DbSet<PaperGenerationJob> PaperGenerationJobs { get; set; }
        public DbSet<PaperJobDocument> PaperJobDocuments { get; set; }

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
                // Avoid relying on DB default constraints in drifted environments.
                // Always send IsActive explicitly from application code.
                entity.Property(e => e.IsActive).ValueGeneratedNever();

                entity.HasOne(c => c.Session)
                    .WithMany(session => session.Classes)
                    .HasForeignKey(c => c.SessionId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // StudentClass configuration
            modelBuilder.Entity<MasterMind.API.Models.Entities.StudentClass>(entity =>
            {
                entity.HasKey(sc => new { sc.StudentId, sc.ClassId });
                entity.Property(e => e.StudentId).IsRequired();
                entity.Property(e => e.ClassId).IsRequired();
                entity.Property(e => e.IsActive).IsRequired();

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

            modelBuilder.Entity<MessageTemplate>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Subject).HasMaxLength(300);
                entity.Property(e => e.Frequency).HasMaxLength(50);
                entity.Property(e => e.Body).IsRequired();
            });

            modelBuilder.Entity<TemplateDispatchLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Channel).HasMaxLength(50);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.RenderedSubject).HasMaxLength(300);
                entity.Property(e => e.RenderedBody).IsRequired();

                entity.HasOne(e => e.MessageTemplate)
                    .WithMany()
                    .HasForeignKey(e => e.MessageTemplateId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Student)
                    .WithMany()
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.StudentFee)
                    .WithMany()
                    .HasForeignKey(e => e.StudentFeeId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.FeeReceipt)
                    .WithMany()
                    .HasForeignKey(e => e.FeeReceiptId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<AdminNote>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Content).IsRequired().HasMaxLength(4000);
                entity.Property(e => e.NoteDate).HasColumnType("date");
            });

            modelBuilder.Entity<AcademicPlannerEntry>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SchoolName).HasMaxLength(200);
                entity.Property(e => e.Subject).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Syllabus).IsRequired();
                entity.Property(e => e.ExamDate).HasColumnType("date");
                entity.Property(e => e.StartTime).HasColumnType("time");
                entity.Property(e => e.EndTime).HasColumnType("time");
                entity.Property(e => e.Notes).HasMaxLength(2000);

                entity.HasOne(e => e.Session)
                    .WithMany()
                    .HasForeignKey(e => e.SessionId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Student)
                    .WithMany()
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Class)
                    .WithMany()
                    .HasForeignKey(e => e.ClassId)
                    .OnDelete(DeleteBehavior.SetNull);
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

                entity.HasOne(t => t.User)
                    .WithMany()
                    .HasForeignKey(t => t.UserId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(t => t.Session)
                    .WithMany(session => session.Teachers)
                    .HasForeignKey(t => t.SessionId)
                    .OnDelete(DeleteBehavior.SetNull);
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

            modelBuilder.Entity<PaperDocument>(entity =>
            {
                entity.Property(e => e.FileName).HasMaxLength(260);
                entity.Property(e => e.ContentType).HasMaxLength(120);
                entity.Property(e => e.BlobContainer).HasMaxLength(100);
                entity.Property(e => e.BlobName).HasMaxLength(700);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(40);
                entity.Property(e => e.ErrorMessage).HasMaxLength(1000);

                entity.HasIndex(e => new { e.SessionId, e.UploadedByUserId, e.CreatedAt });
                entity.HasIndex(e => e.BlobName).IsUnique();

                entity.HasOne(e => e.Session)
                    .WithMany()
                    .HasForeignKey(e => e.SessionId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.UploadedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.UploadedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<PaperExtractedQuestion>(entity =>
            {
                entity.Property(e => e.Subject).HasMaxLength(100);
                entity.Property(e => e.ClassName).HasMaxLength(50);
                entity.Property(e => e.Chapter).HasMaxLength(160);
                entity.Property(e => e.QuestionType).HasConversion<string>().HasMaxLength(40);
                entity.Property(e => e.Difficulty).HasConversion<string>().HasMaxLength(40);
                entity.Property(e => e.QuestionText).HasMaxLength(4000);
                entity.Property(e => e.AnswerText).HasMaxLength(4000);
                entity.Property(e => e.SourceMode).HasMaxLength(50);

                entity.HasIndex(e => new { e.SessionId, e.Subject, e.ClassName, e.QuestionType });

                entity.HasOne(e => e.Session)
                    .WithMany()
                    .HasForeignKey(e => e.SessionId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.SourceDocument)
                    .WithMany(d => d.ExtractedQuestions)
                    .HasForeignKey(e => e.SourceDocumentId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<PaperGenerationJob>(entity =>
            {
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(40);
                entity.Property(e => e.StatusMessage).HasMaxLength(300);
                entity.Property(e => e.ClassName).HasMaxLength(50);
                entity.Property(e => e.Subject).HasMaxLength(100);
                entity.Property(e => e.Chapter).HasMaxLength(160);
                entity.Property(e => e.SettingsJson).HasColumnType("nvarchar(max)");
                entity.Property(e => e.AiModelUsed).HasMaxLength(120);
                entity.Property(e => e.ErrorMessage).HasMaxLength(1500);
                entity.Property(e => e.GeneratedPaperBlobContainer).HasMaxLength(100);
                entity.Property(e => e.GeneratedPaperBlobName).HasMaxLength(700);
                entity.Property(e => e.AnswerKeyBlobContainer).HasMaxLength(100);
                entity.Property(e => e.AnswerKeyBlobName).HasMaxLength(700);

                entity.HasIndex(e => new { e.SessionId, e.RequestedByUserId, e.CreatedAt });

                entity.HasOne(e => e.Session)
                    .WithMany()
                    .HasForeignKey(e => e.SessionId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.RequestedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.RequestedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<PaperJobDocument>(entity =>
            {
                entity.HasKey(e => new { e.PaperGenerationJobId, e.PaperDocumentId });

                entity.HasOne(e => e.PaperGenerationJob)
                    .WithMany(j => j.JobDocuments)
                    .HasForeignKey(e => e.PaperGenerationJobId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.PaperDocument)
                    .WithMany(d => d.JobDocuments)
                    .HasForeignKey(e => e.PaperDocumentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
