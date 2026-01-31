using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WhiskeyTracker.Web.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        // Connection string doesn't need to work for migration generation, just needs to be valid format
        optionsBuilder.UseNpgsql("Host=localhost;Database=whiskey_migration_gen;Username=postgres;Password=password");

        return new AppDbContext(optionsBuilder.Options);
    }
}
