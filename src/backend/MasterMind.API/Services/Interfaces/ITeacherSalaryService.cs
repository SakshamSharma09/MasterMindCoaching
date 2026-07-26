using MasterMind.API.Models.Entities;

namespace MasterMind.API.Services.Interfaces;

public interface ITeacherSalaryService
{
    Task EnsureMonthlyObligationsAsync(int? sessionId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TeacherSalary>> GetObligationsAsync(
        int? sessionId = null,
        CancellationToken cancellationToken = default);
}
