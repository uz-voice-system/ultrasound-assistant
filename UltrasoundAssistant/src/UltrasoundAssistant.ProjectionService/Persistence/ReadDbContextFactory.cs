using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace UltrasoundAssistant.ProjectionService.Persistence;

public sealed class ReadDbContextFactory : IDesignTimeDbContextFactory<ReadDbContext>
{
    public ReadDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ReadDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5434;Database=read_db;Username=read_user;Password=read_pass");
        return new ReadDbContext(optionsBuilder.Options);
    }
}
