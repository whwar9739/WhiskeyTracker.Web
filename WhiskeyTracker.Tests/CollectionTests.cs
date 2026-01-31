using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WhiskeyTracker.Web.Data;
using WhiskeyTracker.Web.Pages.Whiskies;
using Xunit;

namespace WhiskeyTracker.Tests;

public class CollectionTests : TestBase
{
    // Helpers removed - inherited from TestBase

    [Fact]
    public async Task Index_RuntimeMigration_CreatesDefaultCollection_ForNewUser()
    {
        // 1. ARRANGE
        using var context = GetInMemoryContext();
        var userId = "new-user-123";

        var legacyService = new WhiskeyTracker.Web.Services.LegacyMigrationService(context);
        var pageModel = new IndexModel(context, legacyService);
        SetMockUser(pageModel, userId);

        // 2. ACT
        await pageModel.OnGetAsync();

        // 3. ASSERT
        var collection = await context.Collections.FirstOrDefaultAsync();
        Assert.NotNull(collection);
        Assert.Equal("My Home Bar", collection.Name);

        var member = await context.CollectionMembers.FirstOrDefaultAsync();
        Assert.NotNull(member);
        Assert.Equal(userId, member.UserId);
        Assert.Equal(CollectionRole.Owner, member.Role);
        Assert.Equal(collection.Id, member.CollectionId);
    }

    [Fact]
    public async Task Index_RuntimeMigration_AdoptsOrphanBottles()
    {
        // 1. ARRANGE
        using var context = GetInMemoryContext();
        var userId = "legacy-user-456";

        // Create orphan bottles (UserId set, CollectionId null)
        context.Whiskies.Add(new Whiskey { Id = 1, Name = "Test Whiskey" });
        context.Bottles.Add(new Bottle { Id = 101, WhiskeyId = 1, UserId = userId, CollectionId = null });
        context.Bottles.Add(new Bottle { Id = 102, WhiskeyId = 1, UserId = userId, CollectionId = null });
        // Bottle belonging to ANOTHER user (should not be touched)
        context.Bottles.Add(new Bottle { Id = 999, WhiskeyId = 1, UserId = "other-user", CollectionId = null });
        await context.SaveChangesAsync();

        var legacyService = new WhiskeyTracker.Web.Services.LegacyMigrationService(context);
        var pageModel = new IndexModel(context, legacyService);
        SetMockUser(pageModel, userId);

        // 2. ACT
        await pageModel.OnGetAsync();

        // 3. ASSERT
        // User should have a collection now
        var member = await context.CollectionMembers.FirstOrDefaultAsync(m => m.UserId == userId);
        Assert.NotNull(member);
        var collectionId = member.CollectionId;

        // Orphans should be adopted
        var bottles = await context.Bottles.Where(b => b.UserId == userId).ToListAsync();
        Assert.All(bottles, b => Assert.Equal(collectionId, b.CollectionId));

        // Other user's bottle remains orphan
        var otherBottle = await context.Bottles.FindAsync(999);
        Assert.Null(otherBottle.CollectionId);
    }

    [Fact]
    public async Task Details_RestrictsAccess_ToCollectionMembers()
    {
        // 1. ARRANGE
        using var context = GetInMemoryContext();
        var userId = "viewer-user";

        var whiskey = new Whiskey { Id = 1, Name = "Shared Whiskey" };
        context.Whiskies.Add(whiskey);

        var colA = new Collection { Id = 10, Name = "My Collection" };
        var colB = new Collection { Id = 20, Name = "Other Collection" };
        context.Collections.AddRange(colA, colB);

        // User Is Member of ColA, NOT ColB
        context.CollectionMembers.Add(new CollectionMember { CollectionId = 10, UserId = userId });
        
        // Bottle in ColA (Should See)
        context.Bottles.Add(new Bottle { Id = 100, WhiskeyId = 1, CollectionId = 10 });
        // Bottle in ColB (Should NOT See)
        context.Bottles.Add(new Bottle { Id = 200, WhiskeyId = 1, CollectionId = 20 });
        
        await context.SaveChangesAsync();
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var legacyService = new WhiskeyTracker.Web.Services.LegacyMigrationService(context);
        var pageModel = new DetailsModel(context);
        SetMockUser(pageModel, userId);

        // 2. ACT
        await pageModel.OnGetAsync(1);

        // 3. ASSERT
        Assert.NotNull(pageModel.Whiskey);
        Assert.Contains(pageModel.Whiskey.Bottles, b => b.Id == 100);
        Assert.DoesNotContain(pageModel.Whiskey.Bottles, b => b.Id == 200);
    }
}
