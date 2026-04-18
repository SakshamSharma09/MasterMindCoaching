using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MasterMind.API.Data;

/// <summary>
/// Design-time factory for dotnet ef migrations against SQL Server.
/// Override connection with environment variable MASTERMIND_DESIGN_CONNECTION.
/// </summary>
public class MasterMindDbContextFactory : IDesignTimeDbContextFactory<MasterMindDbContext>
{
    public MasterMindDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MasterMindDbContext>();
        var connectionString = Environment.GetEnvironmentVariable("MASTERMIND_DESIGN_CONNECTION")
            ?? "Server=(localdb)\\mssqllocaldb;Database=MasterMindDesign;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";
        optionsBuilder.UseSqlServer(connectionString);
        return new MasterMindDbContext(optionsBuilder.Options);
    }
}
