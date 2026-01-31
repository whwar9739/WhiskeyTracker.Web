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
