using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WhiskeyTracker.Web.Data;
using WhiskeyTracker.Web.Pages.Whiskies;

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
        // 1. ARRANGE
        using var context = GetInMemoryContext();
        context.Whiskies.Add(new Whiskey { Id = 1, Name = "Parent Whiskey" });
        await context.SaveChangesAsync();

        var fixedTime = new DateTimeOffset(2015, 10, 21, 0, 0, 0, TimeSpan.Zero);
        var fakeTime = new FakeTimeProvider(fixedTime);

        var pageModel = new AddBottleModel(context, fakeTime);

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

    // --- Helper to Mock User ---
    private void SetMockUser(PageModel page, string userId)
    {
        var claims = new List<System.Security.Claims.Claim>
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, userId)
        };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);

        page.PageContext = new Microsoft.AspNetCore.Mvc.RazorPages.PageContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
            {
                User = claimsPrincipal
            }
        };
    }

    [Fact]
    public async Task AddBottle_SavesNewBottle_OnPost()
    {
        using var context = GetInMemoryContext();
        context.Whiskies.Add(new Whiskey { Id = 1, Name = "Parent" });
        await context.SaveChangesAsync();

        // Use FakeTimeProvider with any date, since this test doesn't check the date
        var pageModel = new AddBottleModel(context, new FakeTimeProvider(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)))
        {
            NewBottle = new Bottle { WhiskeyId = 1, PurchaseDate = null }
        };

        // MOCK USER
        SetMockUser(pageModel, "test-user-id");

        var result = await pageModel.OnPostAsync();

        Assert.IsType<RedirectToPageResult>(result);
        Assert.Single(context.Bottles);
        var savedBottle = await context.Bottles.FirstAsync();
        Assert.Equal("test-user-id", savedBottle.UserId);
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

