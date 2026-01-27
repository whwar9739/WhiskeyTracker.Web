using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.DataProtection;
using WhiskeyTracker.Web.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/");
});

// ---------------------------------------------------------
// 1. DATABASE CONFIGURATION (The Switchboard)
// ---------------------------------------------------------
// builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddDataProtection()
    .PersistKeysToDbContext<AppDbContext>();

// Email Service
builder.Services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, WhiskeyTracker.Web.Services.EmailSender>();

// Authentication
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? throw new InvalidOperationException("Authentication:Google:ClientId is not configured.");
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? throw new InvalidOperationException("Authentication:Google:ClientSecret is not configured.");
    });

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
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

    // SECURITY: Restrict trusted proxies to known private networks to prevent IP spoofing.
    // In Kubernetes, the Ingress Controller behaves as the proxy.
    // Since we don't know the exact Pod CIDR, we trust standard private ranges.
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
    options.KnownNetworks.Add(new IPNetwork(System.Net.IPAddress.Parse("10.0.0.0"), 8));
    options.KnownNetworks.Add(new IPNetwork(System.Net.IPAddress.Parse("172.16.0.0"), 12));
    options.KnownNetworks.Add(new IPNetwork(System.Net.IPAddress.Parse("192.168.0.0"), 16));
});
builder.Services.AddSingleton(TimeProvider.System);

var app = builder.Build();

// ---------------------------------------------------------
// 2. DATA SEEDING (Config Driven)
// ---------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    
    // Migrate() applies any pending migrations and creates the DB if it doesn't exist
    // context.Database.Migrate(); // NOTE: Ensure this is safe for your environment

    // Only seed if the configuration explicitly says 'true'
    if (dbSection.GetValue<bool>("SeedOnStartup"))
    {
        Console.WriteLine("--> Seeding Data...");
        await DbInitializer.Initialize(context, userManager);
    }
}


app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapRazorPages();

app.Run();