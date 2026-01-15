// WhiskeyTracker.Web/Data/AppDbContext.cs
using Microsoft.EntityFrameworkCore;

namespace WhiskeyTracker.Web.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Whiskey> Whiskies { get; set; }
    public DbSet<Bottle> Bottles { get; set; }
    public DbSet<TastingSession> TastingSessions { get; set; }
    public DbSet<TastingNote> TastingNotes { get; set; }
}