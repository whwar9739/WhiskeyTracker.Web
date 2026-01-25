using Microsoft.AspNetCore.Identity;
using WhiskeyTracker.Web.Data;

namespace WhiskeyTracker.Web.Data;

public static class DbInitializer
{
    public static async Task Initialize(AppDbContext context, UserManager<IdentityUser> userManager)
    {
        // 1. Ensure the DB exists
        context.Database.EnsureCreated();

        // 2. Add Test User
        var testUserEmail = "test@example.com";
        var user = await userManager.FindByEmailAsync(testUserEmail);

        if (user == null)
        {
            user = new IdentityUser { UserName = testUserEmail, Email = testUserEmail, EmailConfirmed = true };
            await userManager.CreateAsync(user, "Password123!");
        }

        // 3. Look for any whiskies.
        if (context.Whiskies.Any())
        {
            return;   // DB has been seeded
        }

        // 4. Add Seed Data
        var whiskies = new Whiskey[]
        {
            new Whiskey 
            { 
                 Name = "Buffalo Trace", 
                 Distillery = "Buffalo Trace", 
                 Region = "Kentucky", 
                 Type = "Bourbon", 
                 ABV = 45,
                 GeneralNotes = "A solid daily drinker. Notes of vanilla, caramel, and a hint of spice."
            },
            new Whiskey 
            { 
                Name = "Lagavulin 16", 
                Distillery = "Lagavulin", 
                Region = "Islay", 
                Type = "Single Malt", 
                ABV = 43,
                GeneralNotes = "The classic Islay peat monster. Strong bonfire smoke, iodine, and dried fruit sweetness."
            },
            new Whiskey 
            { 
                Name = "Redbreast 12", 
                Distillery = "Midleton", 
                Region = "Ireland", 
                Type = "Single Pot Still", 
                ABV = 40,
                GeneralNotes = "Christmas cake in a glass. Rich, spicy, and incredibly smooth."
            },
            new Whiskey 
            { 
                Name = "The Infinity Blend", 
                Distillery = "Home", 
                Region = "My House", 
                Type = "Blend", 
                ABV = 0,
                GeneralNotes = "The forever bottle. Constantly changing profile."
            }
        };

        context.Whiskies.AddRange(whiskies);
        await context.SaveChangesAsync();

        // Add a sample bottle linked to the user
        var bottle = new Bottle 
        { 
            WhiskeyId = whiskies[0].Id, 
            UserId = user.Id,
            Status = BottleStatus.Opened, 
            CapacityMl = 750, 
            CurrentVolumeMl = 400,
            PurchaseDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-2)),
            PurchasePrice = 29.99m
        };
        
        // Add an infinity bottle linked to the user
        var infinity = new Bottle
        {
            WhiskeyId = whiskies[3].Id,
            UserId = user.Id,
            Status = BottleStatus.Opened,
            CapacityMl = 700,
            CurrentVolumeMl = 0,
            IsInfinityBottle = true
        };

        context.Bottles.AddRange(bottle, infinity);
        await context.SaveChangesAsync();
    }
}