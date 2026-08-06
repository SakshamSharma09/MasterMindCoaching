using MasterMind.API.Controllers;
using MasterMind.API.Data;
using MasterMind.API.Models.Entities;
using MasterMind.API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using System.Security.Claims;
using Xunit;

namespace MasterMind.API.Tests;

public class ParentInvitationTests
{
    [Fact]
    public async Task NewStudentReusesParentLoginAfterEarlierStudentWasDeleted()
    {
        var options = new DbContextOptionsBuilder<MasterMindDbContext>()
            .UseInMemoryDatabase($"deleted-student-mobile-reuse-{Guid.NewGuid()}")
            .Options;
        await using var context = new MasterMindDbContext(options);
        var parentRole = new Role { Name = "Parent" };
        var parentUser = new User
        {
            FirstName = "Test",
            LastName = "Parent",
            Email = "mobile_7627053236@placeholder.mastermind.local",
            Mobile = "7627053236",
            IsActive = true
        };
        var activeSession = new Session
        {
            Name = "2026-27",
            DisplayName = "Academic Year 2026-27",
            AcademicYear = "2026-27",
            StartDate = new DateTime(2026, 4, 1),
            EndDate = new DateTime(2027, 3, 31),
            IsActive = true,
            Status = SessionStatus.Active
        };
        context.AddRange(parentRole, parentUser, activeSession);
        await context.SaveChangesAsync();
        context.UserRoles.Add(new UserRole { UserId = parentUser.Id, RoleId = parentRole.Id });
        context.Students.Add(new Student
        {
            FirstName = "Deleted",
            LastName = "Student",
            DateOfBirth = new DateTime(2012, 1, 1),
            AdmissionDate = new DateTime(2026, 4, 1),
            ParentName = "Test Parent",
            ParentMobile = "7627053236",
            ParentUserId = parentUser.Id,
            SessionId = activeSession.Id,
            IsDeleted = true
        });
        await context.SaveChangesAsync();

        var controller = CreateStudentsController(context);
        var newStudent = new Student
        {
            FirstName = "Replacement",
            LastName = "Student",
            DateOfBirth = new DateTime(2013, 2, 2),
            AdmissionDate = new DateTime(2026, 8, 5),
            ParentName = "Test Parent",
            ParentMobile = "+91 76270 53236"
        };

        var response = await controller.CreateStudent(newStudent, activeSession.Id);

        Assert.IsType<CreatedAtActionResult>(response.Result);
        Assert.Equal(parentUser.Id, newStudent.ParentUserId);
        Assert.Equal("7627053236", newStudent.ParentMobile);
        Assert.Single(context.Users);
    }

    [Fact]
    public async Task InvitationStillSucceedsWhenEmailDeliveryThrows()
    {
        var options = new DbContextOptionsBuilder<MasterMindDbContext>()
            .UseInMemoryDatabase($"parent-invitation-{Guid.NewGuid()}")
            .Options;
        await using var context = new MasterMindDbContext(options);
        context.Roles.Add(new Role { Name = "Parent" });
        var student = new Student
        {
            FirstName = "Invite",
            LastName = "Student",
            DateOfBirth = new DateTime(2012, 1, 1),
            ParentName = "Test Parent",
            ParentMobile = "9887258679",
            ParentEmail = "parent@example.com"
        };
        context.Students.Add(student);
        await context.SaveChangesAsync();

        var controller = new StudentsController(
            context,
            NullLogger<StudentsController>.Instance,
            new NoOpBlobStorageService(),
            new ThrowingEmailService(),
            new ConfigurationBuilder().AddInMemoryCollection().Build());
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    new[] { new Claim(ClaimTypes.NameIdentifier, "1") },
                    "Test"))
            }
        };

        var response = await controller.ResendParentInvitation(student.Id);

        Assert.IsType<OkObjectResult>(response.Result);
        Assert.Single(context.AccountInvitations);
        Assert.NotNull(student.ParentUserId);
    }

    [Fact]
    public async Task InvitationRejectsMobileBelongingToPrivilegedUser()
    {
        var options = new DbContextOptionsBuilder<MasterMindDbContext>()
            .UseInMemoryDatabase($"parent-invitation-privileged-{Guid.NewGuid()}")
            .Options;
        await using var context = new MasterMindDbContext(options);
        var adminRole = new Role { Name = "Admin" };
        var parentRole = new Role { Name = "Parent" };
        var admin = new User
        {
            FirstName = "Admin",
            LastName = "User",
            Email = "admin@example.com",
            Mobile = "9887258679"
        };
        context.AddRange(adminRole, parentRole, admin);
        await context.SaveChangesAsync();
        context.UserRoles.Add(new UserRole
        {
            UserId = admin.Id,
            RoleId = adminRole.Id
        });
        var student = new Student
        {
            FirstName = "Invite",
            LastName = "Student",
            DateOfBirth = new DateTime(2012, 1, 1),
            ParentName = "Test Parent",
            ParentMobile = "9887258679"
        };
        context.Students.Add(student);
        await context.SaveChangesAsync();

        var controller = new StudentsController(
            context,
            NullLogger<StudentsController>.Instance,
            new NoOpBlobStorageService(),
            new ThrowingEmailService(),
            new ConfigurationBuilder().AddInMemoryCollection().Build());

        var response = await controller.ResendParentInvitation(student.Id);

        Assert.IsType<BadRequestObjectResult>(response.Result);
        Assert.Empty(context.AccountInvitations);
        Assert.Null(student.ParentUserId);
    }

    private sealed class ThrowingEmailService : IEmailService
    {
        public Task<bool> SendOtpEmailAsync(string email, string otp) =>
            throw new InvalidOperationException("Email unavailable");

        public Task<bool> SendEmailAsync(string to, string subject, string body) =>
            throw new InvalidOperationException("Email unavailable");

        public bool IsValidEmail(string email) => true;
    }

    private static StudentsController CreateStudentsController(MasterMindDbContext context) =>
        new(
            context,
            NullLogger<StudentsController>.Instance,
            new NoOpBlobStorageService(),
            new ThrowingEmailService(),
            new ConfigurationBuilder().AddInMemoryCollection().Build());

    private sealed class NoOpBlobStorageService : IBlobStorageService
    {
        public Task<string> UploadPhotoAsync(Stream fileStream, string fileName, string contentType) =>
            Task.FromResult(string.Empty);

        public Task<Stream?> DownloadPhotoAsync(string blobName) =>
            Task.FromResult<Stream?>(null);

        public Task<bool> DeletePhotoAsync(string blobName) => Task.FromResult(true);

        public string GetPhotoUrl(string blobName) => string.Empty;
    }
}
