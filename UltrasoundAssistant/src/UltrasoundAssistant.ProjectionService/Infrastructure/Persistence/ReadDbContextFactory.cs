using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace UltrasoundAssistant.ProjectionService.Infrastructure.Persistence;

public sealed class ReadDbContextFactory : IDesignTimeDbContextFactory<ProjectionDbContext>
{
    public ProjectionDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ProjectionDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5434;Database=read_db;Username=read_user;Password=read_pass");
        return new ProjectionDbContext(optionsBuilder.Options);
    }
}
