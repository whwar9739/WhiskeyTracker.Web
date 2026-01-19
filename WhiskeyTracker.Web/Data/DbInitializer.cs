using WhiskeyTracker.Web.Data;

namespace WhiskeyTracker.Web.Data;

public static class DbInitializer
{
    public static void Initialize(AppDbContext context)
    {
        // 1. Ensure the DB exists
        context.Database.EnsureCreated();

        // 2. Look for any whiskies.
        if (context.Whiskies.Any())
        {
            return;   // DB has been seeded
        }

        // 3. Add Seed Data
        var whiskies = new Whiskey[]
        {
            new Whiskey { Name = "Buffalo Trace", Distillery = "Buffalo Trace", Region = "Kentucky", Type = "Bourbon", ABV = 45 },
            new Whiskey { Name = "Lagavulin 16", Distillery = "Lagavulin", Region = "Islay", Type = "Single Malt", ABV = 43 },
            new Whiskey { Name = "Redbreast 12", Distillery = "Midleton", Region = "Ireland", Type = "Single Pot Still", ABV = 40 },
            new Whiskey { Name = "The Infinity Blend", Distillery = "Home", Region = "My House", Type = "Blend", ABV = 0 }
        };

        context.Whiskies.AddRange(whiskies);
        context.SaveChanges();

        // Add a sample bottle
        var bottle = new Bottle 
        { 
            WhiskeyId = whiskies[0].Id, 
            Status = BottleStatus.Opened, 
            CapacityMl = 750, 
            CurrentVolumeMl = 400,
            PurchaseDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-2)),
            PurchasePrice = 29.99m
        };
        
        // Add an infinity bottle
        var infinity = new Bottle
        {
            WhiskeyId = whiskies[3].Id,
            Status = BottleStatus.Opened,
            CapacityMl = 700,
            CurrentVolumeMl = 0,
            IsInfinityBottle = true
        };

        context.Bottles.AddRange(bottle, infinity);
        context.SaveChanges();
    }
}