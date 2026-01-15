using Microsoft.AspNetCore.Mvc;
using WhiskeyTracker.Web.Data;
using WhiskeyTracker.Web.Pages.Tasting;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Moq;

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

    [Fact]
    public async Task Create_SavesAndRedirectsToWizard()
    {
        using var context = GetInMemoryContext();

        // Pass the fake provider to the constructor
        var pageModel = new CreateModel(context, new FakeTimeProvider(DateTimeOffset.Now))
        {
            Session = new TastingSession { Title = "Epic Night" }
        };

        var result = await pageModel.OnPostAsync();

        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("./Wizard", redirect.PageName);

        var savedSession = await context.TastingSessions.FirstAsync();
        Assert.Equal("Epic Night", savedSession.Title);
        Assert.Equal(savedSession.Id, redirect.RouteValues["sessionId"]);
    }
}