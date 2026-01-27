using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WhiskeyTracker.Web.Data;

public class AppDbContext : IdentityDbContext, IDataProtectionKeyContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Whiskey> Whiskies { get; set; }
    public DbSet<Bottle> Bottles { get; set; }
    public DbSet<TastingSession> TastingSessions { get; set; }
    public DbSet<TastingNote> TastingNotes { get; set; }
    public DbSet<BlendComponent> BlendComponents { get; set; }

    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
}