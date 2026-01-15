using Microsoft.AspNetCore.Mvc;
using WhiskeyTracker.Web.Data;
using WhiskeyTracker.Web.Pages.Tasting;
using Xunit;
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
        using var context = GetInMemoryContext();
        var pageModel = new CreateModel(context);

        pageModel.OnGet();

        Assert.NotNull(pageModel.Session);
        Assert.Equal(DateOnly.FromDateTime(DateTime.Today), pageModel.Session.Date);
        Assert.Contains("Tasting on", pageModel.Session.Title);
    }

    [Fact]
    public async Task Create_SavesAndRedirectsToWizard()
    {
        using var context = GetInMemoryContext();
        var pageModel = new CreateModel(context)
        {
            Session = new TastingSession { Title = "Epic Night" }
        };

        var result = await pageModel.OnPostAsync();

        // Should redirect to Wizard with the new ID
        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("./Wizard", redirect.PageName);
        
        var savedSession = await context.TastingSessions.FirstAsync();
        Assert.Equal("Epic Night", savedSession.Title);
        
        // Verify the ID in the redirect matches the saved ID
        Assert.Equal(savedSession.Id, redirect.RouteValues["sessionId"]);
    }
}