using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Moq;
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

    private Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> GetMockUserManager(AppDbContext context)
    {
        var store = new Mock<Microsoft.AspNetCore.Identity.IUserStore<ApplicationUser>>();
        var mockUserManager = new Mock<Microsoft.AspNetCore.Identity.UserManager<ApplicationUser>>(
            store.Object, null, null, null, null, null, null, null, null);
        
        // Setup GetUserId to return the user ID from claims
        mockUserManager.Setup(um => um.GetUserId(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
            .Returns<System.Security.Claims.ClaimsPrincipal>(principal => 
                principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
        
        // Setup Users property to return the context's Users DbSet
        mockUserManager.Setup(um => um.Users).Returns(context.Users);
        
        return mockUserManager.Object;
    }

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

        var pageModel = new AddBottleModel(context, fakeTime, GetMockUserManager(context));
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
        var pageModel = new AddBottleModel(context, new FakeTimeProvider(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)), GetMockUserManager(context))
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

        var pageModel = new EditBottleModel(context, GetMockUserManager(context))
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

        var pageModel = new EditBottleModel(context, GetMockUserManager(context))
        {
            Bottle = new Bottle { Id = 999 }
        };
        SetMockUser(pageModel, "test-user");

        var result = await pageModel.OnPostAsync();
        Assert.IsType<NotFoundResult>(result);
    }

}

