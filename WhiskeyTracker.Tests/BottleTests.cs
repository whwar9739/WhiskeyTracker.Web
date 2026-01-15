using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WhiskeyTracker.Web.Data;
using WhiskeyTracker.Web.Pages.Whiskies;
using Xunit;

namespace WhiskeyTracker.Tests;

public class BottleTests
{
    private AppDbContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task AddBottle_PopulatesDefaults_OnGet()
    {
        using var context = GetInMemoryContext();
        context.Whiskies.Add(new Whiskey { Id = 1, Name = "Parent Whiskey" });
        await context.SaveChangesAsync();

        var pageModel = new AddBottleModel(context);
        await pageModel.OnGetAsync(1);

        Assert.Equal("Parent Whiskey", pageModel.WhiskeyName);
        Assert.Equal(1, pageModel.NewBottle.WhiskeyId);
        Assert.Equal(DateOnly.FromDateTime(DateTime.Today), pageModel.NewBottle.PurchaseDate);
    }

    [Fact]
    public async Task AddBottle_SavesNewBottle_OnPost()
    {
        using var context = GetInMemoryContext();
        context.Whiskies.Add(new Whiskey { Id = 1, Name = "Parent" });
        await context.SaveChangesAsync();

        var pageModel = new AddBottleModel(context)
        {
            NewBottle = new Bottle { WhiskeyId = 1, PurchaseDate = DateOnly.MinValue }
        };

        var result = await pageModel.OnPostAsync();

        Assert.IsType<RedirectToPageResult>(result);
        Assert.Single(context.Bottles);
    }

    [Fact]
    public async Task EditBottle_UpdatesStatus_OnPost()
    {
        using var context = GetInMemoryContext();
        context.Bottles.Add(new Bottle { Id = 10, Status = BottleStatus.Full });
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();

        var pageModel = new EditBottleModel(context)
        {
            Bottle = new Bottle { Id = 10, Status = BottleStatus.Opened }
        };

        // We must attach the entity to simulate it being tracked in a real web request
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

        var pageModel = new EditBottleModel(context)
        {
            Bottle = new Bottle { Id = 999 }
        };

        var result = await pageModel.OnPostAsync();
        Assert.IsType<NotFoundResult>(result);
    }
}