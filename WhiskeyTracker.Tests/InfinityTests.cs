using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WhiskeyTracker.Web.Data;
using WhiskeyTracker.Web.Pages.Bottles;
using Xunit;

namespace WhiskeyTracker.Tests;

public class InfinityTests : TestBase
{
    [Fact]
    public async Task OnGetAsync_ReturnsNotFound_WhenIdIsNull()
    {
        // Actually the argument is int, so it can't be null, but let's test invalid ID
        using var context = GetInMemoryContext();
        var pageModel = new InfinityModel(context);
        SetMockUser(pageModel, "test-user");

        var result = await pageModel.OnGetAsync(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task OnGetAsync_ReturnsNotFound_WhenUserNotAuthorizedForCollection()
    {
        using var context = GetInMemoryContext();
        
        // Setup: A bottle in a collection the user is NOT a member of
        var whiskey = new Whiskey { Name = "Forbidden Dram" };
        var collection = new Collection { Name = "Other People's Property" };
        var bottle = new Bottle { Whiskey = whiskey, Collection = collection, IsInfinityBottle = true };
        
        context.Bottles.Add(bottle);
        await context.SaveChangesAsync();

        var pageModel = new InfinityModel(context);
        SetMockUser(pageModel, "some-stranger");

        var result = await pageModel.OnGetAsync(bottle.Id);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task OnGetAsync_RedirectsToDetails_WhenNotInfinityBottle()
    {
        using var context = GetInMemoryContext();
        
        // Setup: A regular bottle
        var whiskey = new Whiskey { Name = "Regular Dram" };
        var collection = new Collection { Name = "My Bar" };
        context.Collections.Add(collection);
        
        // Add user as member
        context.CollectionMembers.Add(new CollectionMember 
        { 
            CollectionId = collection.Id, 
            UserId = "test-user", 
            Role = CollectionRole.Owner 
        });

        var bottle = new Bottle 
        { 
            Whiskey = whiskey, 
            Collection = collection, 
            IsInfinityBottle = false // Not an infinity bottle
        };
        
        context.Bottles.Add(bottle);
        await context.SaveChangesAsync();

        var pageModel = new InfinityModel(context);
        SetMockUser(pageModel, "test-user");

        var result = await pageModel.OnGetAsync(bottle.Id);

        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Whiskies/Details", redirect.PageName);
        Assert.Equal(whiskey.Id, redirect.RouteValues?["id"]);
    }

    [Fact]
    public async Task OnGetAsync_LoadsBlendComponents_WhenValid()
    {
        using var context = GetInMemoryContext();
        
        // Setup: Infinity bottle and components
        var whiskey = new Whiskey { Name = "Infinity Blend" };
        var sourceWhiskey = new Whiskey { Name = "Source Malt" };
        var collection = new Collection { Name = "My Bar" };
        
        context.Collections.Add(collection);
        context.Whiskies.AddRange(whiskey, sourceWhiskey);
        
        // Add user as member
        context.CollectionMembers.Add(new CollectionMember 
        { 
            CollectionId = collection.Id, 
            UserId = "test-user", 
            Role = CollectionRole.Owner 
        });

        var infinityBottle = new Bottle 
        { 
            Whiskey = whiskey, 
            Collection = collection, 
            IsInfinityBottle = true 
        };
        
        var sourceBottle = new Bottle
        {
            Whiskey = sourceWhiskey,
            Collection = collection
        };
        
        context.Bottles.AddRange(infinityBottle, sourceBottle);
        await context.SaveChangesAsync(); // Save to get IDs

        // Add pours
        var pour1 = new BlendComponent 
        { 
            InfinityBottleId = infinityBottle.Id, 
            SourceBottleId = sourceBottle.Id,
            AmountAddedMl = 50,
            DateAdded = DateOnly.FromDateTime(DateTime.Now)
        };
        
        context.BlendComponents.Add(pour1);
        await context.SaveChangesAsync();

        var pageModel = new InfinityModel(context);
        SetMockUser(pageModel, "test-user");

        var result = await pageModel.OnGetAsync(infinityBottle.Id);

        Assert.IsType<PageResult>(result);
        Assert.NotNull(pageModel.Bottle);
        Assert.Single(pageModel.BlendComponents);
        Assert.Equal(50, pageModel.BlendComponents[0].AmountAddedMl);
        Assert.Equal("Source Malt", pageModel.BlendComponents[0].SourceBottle?.Whiskey?.Name);
    }
}
