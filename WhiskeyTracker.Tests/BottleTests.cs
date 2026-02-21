using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Moq;
using WhiskeyTracker.Web.Data;
using WhiskeyTracker.Web.Pages.Whiskies;

namespace WhiskeyTracker.Tests;

public class BottleTests : TestBase
{
    // Helpers removed - inherited from TestBase

    [Fact]
    public async Task AddBottle_PopulatesDefaults_OnGet()
    {
        // 1. ARRANGE
        using var context = GetInMemoryContext();
        context.Whiskies.Add(new Whiskey { Id = 1, Name = "Parent Whiskey" });
        
        // Seed collection for the user
        var collection = new Collection { Id = 1, Name = "Test Collection" };
        context.Collections.Add(collection);
        context.CollectionMembers.Add(new CollectionMember 
        { 
            CollectionId = 1, 
            UserId = "test-user-id", 
            Role = CollectionRole.Owner 
        });
        await context.SaveChangesAsync();

        // Seed user for UserManager
        context.Users.Add(new ApplicationUser { Id = "test-user-id", UserName = "test@example.com", DisplayName = "Test User" });
        await context.SaveChangesAsync();

        var fixedTime = new DateTimeOffset(2015, 10, 21, 0, 0, 0, TimeSpan.Zero);
        var fakeTime = new FakeTimeProvider(fixedTime);
        var userManager = GetMockUserManager(context);
        var collectionService = new WhiskeyTracker.Web.Services.CollectionViewModelService(context, userManager);

        var pageModel = new AddBottleModel(context, fakeTime, userManager, collectionService);
        SetMockUser(pageModel, "test-user-id");

        // 2. ACT
        await pageModel.OnGetAsync(1);

        // 3. ASSERT
        Assert.Equal("Parent Whiskey", pageModel.WhiskeyName);
        Assert.Equal(new DateOnly(2015, 10, 21), pageModel.NewBottle.PurchaseDate);
        Assert.Equal(1, pageModel.NewBottle.WhiskeyId);

        // --- NEW CHECKS ---
        Assert.Equal(750, pageModel.NewBottle.CapacityMl);      // Default Capacity
        Assert.Equal(750, pageModel.NewBottle.CurrentVolumeMl); // Default Volume
        Assert.False(pageModel.NewBottle.IsInfinityBottle);     // Default False
    }



    [Fact]
    public async Task AddBottle_SavesNewBottle_OnPost()
    {
        using var context = GetInMemoryContext();
        context.Whiskies.Add(new Whiskey { Id = 1, Name = "Parent" });
        await context.SaveChangesAsync();

        // Use FakeTimeProvider with any date, since this test doesn't check the date
        var userManager = GetMockUserManager(context);
        var collectionService = new WhiskeyTracker.Web.Services.CollectionViewModelService(context, userManager);
        
        var pageModel = new AddBottleModel(context, new FakeTimeProvider(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)), userManager, collectionService)
        {
            NewBottle = new Bottle { WhiskeyId = 1, PurchaseDate = null }
        };

        // SEED COLLECTION
        context.Collections.Add(new Collection { Id = 1, Name = "Test" });
        context.CollectionMembers.Add(new CollectionMember { CollectionId = 1, UserId = "test-user-id", Role = CollectionRole.Owner });
        await context.SaveChangesAsync();

        // MOCK USER
        SetMockUser(pageModel, "test-user-id");

        // Set the Purchaser explicitly (simulating form selection)
        pageModel.NewBottle.UserId = "test-user-id";
        pageModel.NewBottle.CollectionId = 1;

        var result = await pageModel.OnPostAsync();

        Assert.IsType<RedirectToPageResult>(result);
        Assert.Single(context.Bottles);
        var savedBottle = await context.Bottles.FirstAsync();
        Assert.Equal("test-user-id", savedBottle.UserId);
        Assert.Equal(1, savedBottle.CollectionId);
    }

    [Fact]
    public async Task EditBottle_UpdatesStatus_OnPost()
    {
        using var context = GetInMemoryContext();
        // SEED COLLECTION
        var collection = new Collection { Id = 1, Name = "Test" };
        context.Collections.Add(collection);
        context.CollectionMembers.Add(new CollectionMember { CollectionId = 1, UserId = "test-user", Role = CollectionRole.Owner });
        
        // Seed whiskey
        context.Whiskies.Add(new Whiskey { Id = 1, Name = "Test Whiskey" });
        context.Bottles.Add(new Bottle { Id = 10, WhiskeyId = 1, Status = BottleStatus.Full, CollectionId = 1, UserId = "test-user" });
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();

        var userManager = GetMockUserManager(context);
        var collectionService = new WhiskeyTracker.Web.Services.CollectionViewModelService(context, userManager);

        var pageModel = new EditBottleModel(context, userManager, collectionService)
        {
            Bottle = new Bottle { Id = 10, WhiskeyId = 1, Status = BottleStatus.Opened, CollectionId = 1, UserId = "test-user" }
        };
        SetMockUser(pageModel, "test-user");

        var result = await pageModel.OnPostAsync();

        Assert.IsType<RedirectToPageResult>(result);
        var savedBottle = await context.Bottles.FindAsync(10);
        Assert.NotNull(savedBottle);
        Assert.Equal(BottleStatus.Opened, savedBottle.Status);
    }

    [Fact]
    public async Task EditBottle_ReturnsNotFound_IfBottleDeletedConcurrency()
    {
        using var context = GetInMemoryContext();
        // Database is empty (simulating someone else deleted the bottle)

        var userManager = GetMockUserManager(context);
        var collectionService = new WhiskeyTracker.Web.Services.CollectionViewModelService(context, userManager);

        var pageModel = new EditBottleModel(context, userManager, collectionService)
        {
            Bottle = new Bottle { Id = 999 }
        };
        SetMockUser(pageModel, "test-user");

        var result = await pageModel.OnPostAsync();
        Assert.IsType<NotFoundResult>(result);
    }

}

