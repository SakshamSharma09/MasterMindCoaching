using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MasterMind.API.Controllers;
using MasterMind.API.Data;
using MasterMind.API.Models.DTOs.Auth;
using MasterMind.API.Models.Entities;
using MasterMind.API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace MasterMind.API.Tests;

public class TeacherPortalAndInvitationTests
{
    [Fact]
    public async Task TeacherInvitationCreatesMobileAccountAndHashedSingleUseToken()
    {
        await using var context = CreateContext();
        context.Roles.Add(new Role { Name = "Teacher" });
        var teacher = new Teacher
        {
            FirstName = "Test",
            LastName = "Teacher",
            Mobile = "9876543210",
            Email = "teacher_9876543210@placeholder.mastermind.local",
            IsActive = true
        };
        context.Teachers.Add(teacher);
        await context.SaveChangesAsync();

        var controller = new TeachersController(
            context,
            new NoOpTeacherSalaryService(),
            new NoOpEmailService(),
            new ConfigurationBuilder().AddInMemoryCollection().Build(),
            NullLogger<TeachersController>.Instance)
        {
            ControllerContext = AuthenticatedControllerContext(99)
        };

        var result = await controller.CreateTeacherInvitation(teacher.Id);

        Assert.IsType<OkObjectResult>(result.Result);
        var invitation = Assert.Single(context.AccountInvitations);
        Assert.Equal(64, invitation.TokenHash.Length);
        Assert.True(invitation.ExpiresAt > DateTime.UtcNow.AddHours(71));
        var user = await context.Users.Include(u => u.UserRoles).SingleAsync();
        Assert.Equal("9876543210", user.Mobile);
        Assert.Equal(user.Id, teacher.UserId);
        Assert.Single(user.UserRoles);
    }

    [Fact]
    public async Task AssignedTeacherCanUpsertAttendanceWithExpectedClassTimes()
    {
        await using var context = CreateContext();
        var teacher = new Teacher { FirstName = "Test", LastName = "Teacher", Email = "teacher@example.com", Mobile = "9876543210", UserId = 7 };
        var classItem = new MasterMind.API.Models.Entities.Class { Name = "Class 8", AcademicYear = "2026-27", IsActive = true };
        var student = new Student { FirstName = "Test", LastName = "Student", DateOfBirth = new DateTime(2012, 1, 1), ParentName = "Parent", ParentMobile = "9000000000", IsActive = true };
        context.AddRange(teacher, classItem, student);
        await context.SaveChangesAsync();
        context.TeacherClasses.Add(new TeacherClass { TeacherId = teacher.Id, ClassId = classItem.Id, IsActive = true });
        context.StudentClasses.Add(new StudentClass { StudentId = student.Id, ClassId = classItem.Id, IsActive = true });
        await context.SaveChangesAsync();

        var controller = new TeacherPortalController(context, NullLogger<TeacherPortalController>.Instance)
        {
            ControllerContext = AuthenticatedControllerContext(7)
        };
        var date = new DateOnly(2026, 8, 2);

        var first = await controller.SaveClassAttendance(classItem.Id, new TeacherAttendanceRequest
        {
            Date = date,
            Records = { new TeacherAttendanceRecordRequest { StudentId = student.Id, Status = "Present" } }
        });
        Assert.IsType<OkObjectResult>(first.Result);
        var attendance = Assert.Single(context.Attendances);
        Assert.Equal(new TimeOnly(15, 0), attendance.CheckInTime);
        Assert.Equal(new TimeOnly(18, 0), attendance.CheckOutTime);

        await controller.SaveClassAttendance(classItem.Id, new TeacherAttendanceRequest
        {
            Date = date,
            Records = { new TeacherAttendanceRecordRequest { StudentId = student.Id, Status = "Absent" } }
        });
        Assert.Single(context.Attendances);
        Assert.Equal(AttendanceStatus.Absent, attendance.Status);
        Assert.Null(attendance.CheckInTime);
        Assert.Null(attendance.CheckOutTime);
    }

    [Fact]
    public async Task TeacherInvitationCannotBeResentAfterOnboarding()
    {
        await using var context = CreateContext();
        var role = new Role { Name = "Teacher" };
        var user = new User
        {
            FirstName = "Ready",
            LastName = "Teacher",
            Mobile = "9876543210",
            Email = "ready@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123")
        };
        context.AddRange(role, user);
        await context.SaveChangesAsync();
        context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
        var teacher = new Teacher
        {
            FirstName = "Ready",
            LastName = "Teacher",
            Mobile = user.Mobile,
            Email = user.Email,
            UserId = user.Id,
            IsActive = true
        };
        context.Teachers.Add(teacher);
        await context.SaveChangesAsync();

        var controller = new TeachersController(
            context,
            new NoOpTeacherSalaryService(),
            new NoOpEmailService(),
            new ConfigurationBuilder().AddInMemoryCollection().Build(),
            NullLogger<TeachersController>.Instance);

        var result = await controller.CreateTeacherInvitation(teacher.Id);

        Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Empty(context.AccountInvitations);
    }

    [Fact]
    public async Task AcceptingTeacherInvitationUpdatesTeacherRecoveryEmailAndConsumesToken()
    {
        await using var context = CreateContext();
        var role = new Role { Name = "Teacher" };
        var user = new User { FirstName = "Invite", LastName = "Teacher", Mobile = "9876543210", Email = "teacher_9876543210@placeholder.mastermind.local" };
        context.AddRange(role, user);
        await context.SaveChangesAsync();
        context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
        var teacher = new Teacher { FirstName = "Invite", LastName = "Teacher", Mobile = user.Mobile, Email = user.Email, UserId = user.Id, IsActive = true };
        context.Teachers.Add(teacher);
        var rawToken = "teacher-invitation-token";
        var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
        context.AccountInvitations.Add(new AccountInvitation
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddHours(72)
        });
        await context.SaveChangesAsync();

        var controller = new AuthController(
            new NoOpAuthService(),
            new NoOpDeviceService(),
            NullLogger<AuthController>.Instance,
            context,
            new NoOpEmailService(),
            new ConfigurationBuilder().AddInMemoryCollection().Build())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        var result = await controller.AcceptInvitation(new AcceptInvitationRequest
        {
            Token = rawToken,
            Email = "teacher.recovery@example.com",
            Password = "Password123"
        });

        Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal("teacher.recovery@example.com", teacher.Email);
        Assert.Equal("teacher.recovery@example.com", user.Email);
        Assert.True(BCrypt.Net.BCrypt.Verify("Password123", user.PasswordHash));
        Assert.NotNull(context.AccountInvitations.Single().UsedAt);
    }

    private static MasterMindDbContext CreateContext() => new(
        new DbContextOptionsBuilder<MasterMindDbContext>()
            .UseInMemoryDatabase($"teacher-portal-{Guid.NewGuid()}")
            .Options);

    private static ControllerContext AuthenticatedControllerContext(int userId) => new()
    {
        HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) },
                "Test"))
        }
    };

    private sealed class NoOpEmailService : IEmailService
    {
        public Task<bool> SendOtpEmailAsync(string email, string otp) => Task.FromResult(false);
        public Task<bool> SendEmailAsync(string to, string subject, string body) => Task.FromResult(false);
        public bool IsValidEmail(string email) => true;
    }

    private sealed class NoOpTeacherSalaryService : ITeacherSalaryService
    {
        public Task EnsureMonthlyObligationsAsync(int? sessionId = null, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<IReadOnlyList<TeacherSalary>> GetObligationsAsync(int? sessionId = null, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<TeacherSalary>>(Array.Empty<TeacherSalary>());
    }

    private sealed class NoOpAuthService : IAuthService
    {
        public Task<OtpResponseDto> RequestOtpAsync(OtpRequestDto request) => throw new NotImplementedException();
        public Task<AuthResponseDto> VerifyOtpAsync(OtpVerifyDto request) => throw new NotImplementedException();
        public Task<AuthResponseDto> LoginWithPasswordAsync(PasswordLoginDto request) => throw new NotImplementedException();
        public Task<AuthResponseDto> QuickLoginAsync(string email) => throw new NotImplementedException();
        public Task<AuthResponseDto> SetPasswordAsync(int userId, SetPasswordDto request) => throw new NotImplementedException();
        public Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto request) => throw new NotImplementedException();
        public Task<bool> LogoutAsync(int userId, string? refreshToken = null) => throw new NotImplementedException();
        public Task<UserDto?> GetCurrentUserAsync(int userId) => throw new NotImplementedException();
    }

    private sealed class NoOpDeviceService : IDeviceService
    {
        public Task<UserDevice> RegisterDeviceAsync(int userId, string deviceId, string deviceName, string deviceType, string browserInfo, string ipAddress, string location) => throw new NotImplementedException();
        public Task<UserDevice?> GetDeviceAsync(int userId, string deviceId) => throw new NotImplementedException();
        public Task<List<UserDevice>> GetUserDevicesAsync(int userId) => throw new NotImplementedException();
        public Task<bool> IsDeviceTrustedAsync(int userId, string deviceId) => throw new NotImplementedException();
        public Task TrustDeviceAsync(int userId, string deviceId) => throw new NotImplementedException();
        public Task UpdateDeviceActivityAsync(int userId, string deviceId) => throw new NotImplementedException();
        public Task RevokeDeviceAsync(int userId, string deviceId) => throw new NotImplementedException();
        public Task CleanupExpiredDevicesAsync() => throw new NotImplementedException();
    }
}
