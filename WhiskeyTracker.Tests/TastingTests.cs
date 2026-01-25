using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WhiskeyTracker.Web.Data;
using WhiskeyTracker.Web.Pages.Tasting;
using Microsoft.EntityFrameworkCore;

namespace WhiskeyTracker.Tests;

public class TastingTests
{
    private AppDbContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task Create_InitializesDefaults_OnGet()
    {
        // 1. ARRANGE
        using var context = GetInMemoryContext();

        // FIX: Use your concrete Fake class, NOT a Mock
        var fixedTime = new DateTimeOffset(1999, 12, 31, 23, 59, 0, TimeSpan.Zero);
        var fakeTime = new FakeTimeProvider(fixedTime);

        var pageModel = new CreateModel(context, fakeTime);

        // 2. ACT
        pageModel.OnGet();

        // 3. ASSERT
        Assert.NotNull(pageModel.Session);
        // Verify it used the date from your FakeTimeProvider
        Assert.Equal(new DateOnly(1999, 12, 31), pageModel.Session.Date);
        Assert.Equal("Tasting on Dec 31, 1999", pageModel.Session.Title);
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
    public async Task Create_SavesAndRedirectsToWizard()
    {
        using var context = GetInMemoryContext();

        // Pass the fake provider to the constructor
        var pageModel = new CreateModel(context, new FakeTimeProvider(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)))
        {
            Session = new TastingSession { Title = "Epic Night" }
        };

        // MOCK USER
        SetMockUser(pageModel, "test-user-id");

        var result = await pageModel.OnPostAsync();

        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("./Wizard", redirect.PageName);

        var savedSession = await context.TastingSessions.FirstAsync();
        Assert.Equal("Epic Night", savedSession.Title);
        Assert.Equal("test-user-id", savedSession.UserId); // Check UserId
        Assert.NotNull(redirect.RouteValues);
        Assert.Equal(savedSession.Id, redirect.RouteValues["sessionId"]);
    }
}