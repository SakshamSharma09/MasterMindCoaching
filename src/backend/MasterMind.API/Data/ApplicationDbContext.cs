using MasterMind.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace MasterMind.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // User Management
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<UserRole> UserRoles { get; set; } = null!;
    public DbSet<OtpRecord> OtpRecords { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

    // Student Management
    public DbSet<Student> Students { get; set; } = null!;
    public DbSet<Class> Classes { get; set; } = null!;
    public DbSet<StudentClass> StudentClasses { get; set; } = null!;

    // Attendance
    public DbSet<Attendance> Attendances { get; set; } = null!;
    public DbSet<TeacherAttendance> TeacherAttendances { get; set; } = null!;

    // Finance
    public DbSet<FeeStructure> FeeStructures { get; set; } = null!;
    public DbSet<StudentFee> StudentFees { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<TeacherSalary> TeacherSalaries { get; set; } = null!;

    // Teacher
    public DbSet<Teacher> Teachers { get; set; } = null!;
    public DbSet<TeacherClass> TeacherClasses { get; set; } = null!;
    public DbSet<StudentRemark> StudentRemarks { get; set; } = null!;
    public DbSet<StudentPerformance> StudentPerformances { get; set; } = null!;

    // Leads
    public DbSet<Lead> Leads { get; set; } = null!;
    public DbSet<LeadFollowup> LeadFollowups { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from the assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // User Configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Mobile).IsUnique();
        });

        // UserRole Configuration (Many-to-Many)
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(ur => new { ur.UserId, ur.RoleId });

            entity.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            entity.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);
        });

        // StudentClass Configuration (Many-to-Many)
        modelBuilder.Entity<StudentClass>(entity =>
        {
            entity.HasKey(sc => new { sc.StudentId, sc.ClassId });

            entity.HasOne(sc => sc.Student)
                .WithMany(s => s.StudentClasses)
                .HasForeignKey(sc => sc.StudentId);

            entity.HasOne(sc => sc.Class)
                .WithMany(c => c.StudentClasses)
                .HasForeignKey(sc => sc.ClassId);
        });

        // TeacherClass Configuration (Many-to-Many)
        modelBuilder.Entity<TeacherClass>(entity =>
        {
            entity.HasKey(tc => new { tc.TeacherId, tc.ClassId });

            entity.HasOne(tc => tc.Teacher)
                .WithMany(t => t.TeacherClasses)
                .HasForeignKey(tc => tc.TeacherId);

            entity.HasOne(tc => tc.Class)
                .WithMany(c => c.TeacherClasses)
                .HasForeignKey(tc => tc.ClassId);
        });

        // Seed default roles
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Admin", Description = "Administrator with full access" },
            new Role { Id = 2, Name = "Teacher", Description = "Teacher with limited access" },
            new Role { Id = 3, Name = "Parent", Description = "Parent with view-only access" }
        );
    }
}