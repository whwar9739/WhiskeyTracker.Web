using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WhiskeyTracker.Web.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

var dbSection = builder.Configuration.GetSection("Database");
var provider = dbSection["Provider"];
var connectionString = dbSection["ConnectionString"] ?? builder.Configuration.GetConnectionString("DefaultConnection");

Console.WriteLine($"--> Database Provider: {provider}");

switch (provider)
{
    case "postgres":
    case "postgresql":
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));
        break;
    default:
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase("WhiskeyTrackerInMemory"));
        break;
}

builder.Services.AddHealthChecks();
builder.Services.AddSingleton(TimeProvider.System);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();

    if (dbSection.GetValue<bool>("SeedOnStartup"))
    {
        Console.WriteLine("--> Seeding Database...");
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