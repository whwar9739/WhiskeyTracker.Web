using Microsoft.EntityFrameworkCore;
using WhiskeyTracker.Web.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

// ---------------------------------------------------------
// 1. DATABASE CONFIGURATION (The Switchboard)
// ---------------------------------------------------------
var dbSection = builder.Configuration.GetSection("Database");
var provider = dbSection["Provider"]; 
var connectionString = dbSection["ConnectionString"] ?? builder.Configuration.GetConnectionString("DefaultConnection");

Console.WriteLine($"--> Database Provider: {provider}");

switch (provider?.ToLower())
{
    case "postgres":
    case "postgresql":
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));
        break;
    default:
        // Default to In-Memory if config is missing or set to InMemory
        // Ensure you have run: dotnet add package Microsoft.EntityFrameworkCore.InMemory
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase("WhiskeyTrackerInMemory"));
        break;
}

builder.Services.AddHealthChecks();
builder.Services.AddSingleton(TimeProvider.System);

var app = builder.Build();

// ---------------------------------------------------------
// 2. DATA SEEDING (Config Driven)
// ---------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    // Migrate() applies any pending migrations and creates the DB if it doesn't exist
    context.Database.Migrate();

    // Only seed if the configuration explicitly says 'true'
    if (dbSection.GetValue<bool>("SeedOnStartup"))
    {
        Console.WriteLine("--> Seeding Data...");
        DbInitializer.Initialize(context);
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapHealthChecks("/health");
app.MapRazorPages();

app.Run();