using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace UltrasoundAssistant.AuditService.Persistence;

public sealed class AuditDbContextFactory : IDesignTimeDbContextFactory<AuditDbContext>
{
    public AuditDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AuditDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5435;Database=audit_db;Username=audit_user;Password=audit_pass");
        return new AuditDbContext(optionsBuilder.Options);
    }
}
