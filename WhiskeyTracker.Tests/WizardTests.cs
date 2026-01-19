using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
using WhiskeyTracker.Web.Data;
using WhiskeyTracker.Web.Pages.Tasting;
using Xunit;

namespace WhiskeyTracker.Tests;

public class WizardTests
{
    private AppDbContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    // Helper to Initialize PageModel with TempData
    private WizardModel CreateWizardModel(AppDbContext context)
    {
        var httpContext = new DefaultHttpContext();
        var modelState = new ModelStateDictionary();
        var actionContext = new ActionContext(httpContext, new Microsoft.AspNetCore.Routing.RouteData(), new PageActionDescriptor(), modelState);
        var modelMetadataProvider = new EmptyModelMetadataProvider();
        var viewData = new ViewDataDictionary(modelMetadataProvider, modelState);
        var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        var pageContext = new PageContext(actionContext)
        {
            ViewData = viewData
        };

        return new WizardModel(context)
        {
            PageContext = pageContext,
            TempData = tempData,
            Url = new UrlHelper(actionContext)
        };
    }

    [Fact]
    public async Task OnGet_ReturnsNotFound_IfSessionInvalid()
    {
        using var context = GetInMemoryContext();
        var pageModel = CreateWizardModel(context);

        var result = await pageModel.OnGetAsync(999); // Invalid ID

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task OnGet_PopulatesSelectLists_WhenSessionValid()
    {
        // ARRANGE
        using var context = GetInMemoryContext();
        var session = new TastingSession { Date = DateOnly.FromDateTime(DateTime.Now) };
        context.TastingSessions.Add(session);
        var whiskey = new Whiskey { Name = "Test Whiskey" };
        context.Whiskies.Add(whiskey);
        var bottle = new Bottle { WhiskeyId = whiskey.Id, Status = BottleStatus.Opened };
        context.Bottles.Add(bottle);
        await context.SaveChangesAsync();

        var pageModel = CreateWizardModel(context);

        // ACT
        var result = await pageModel.OnGetAsync(session.Id);

        // ASSERT
        Assert.IsType<PageResult>(result);
        Assert.NotNull(pageModel.BottleOptions);
        Assert.NotNull(pageModel.WhiskeyOptions);
        Assert.Single(pageModel.BottleOptions!);
    }

    [Fact]
    public async Task Create_DeductsInventory_WhenBottleSelected()
    {
        // ARRANGE
        using var context = GetInMemoryContext();
        var whiskey = new Whiskey { Name = "Test Whiskey" };
        context.Whiskies.Add(whiskey);
        await context.SaveChangesAsync();

        var bottle = new Bottle 
        { 
            WhiskeyId = whiskey.Id, 
            CurrentVolumeMl = 700, 
            Status = BottleStatus.Opened 
        };
        context.Bottles.Add(bottle);
        var session = new TastingSession { Date = DateOnly.FromDateTime(DateTime.Now) };
        context.TastingSessions.Add(session);
        await context.SaveChangesAsync();

        var pageModel = CreateWizardModel(context);
        pageModel.SelectedBottleId = bottle.Id;
        pageModel.NewNote = new TastingNote 
        { 
            Notes = "Yummy", 
            Rating = 8,
            PourAmountMl = 50 // <--- User drank 50ml
        };

        // ACT
        await pageModel.OnPostAsync(session.Id);

        // ASSERT
        var dbBottle = await context.Bottles.FindAsync(bottle.Id);
        Assert.NotNull(dbBottle);
        Assert.Equal(650, dbBottle.CurrentVolumeMl); // 700 - 50 = 650
    }

    [Fact]
    public async Task Create_FinishesBottle_WhenVolumeHitsZero()
    {
        // ARRANGE
        using var context = GetInMemoryContext();
        var whiskey = new Whiskey { Name = "Test Whiskey" }; // Ensure whiskey exists
        context.Whiskies.Add(whiskey);
        await context.SaveChangesAsync();

        var bottle = new Bottle 
        { 
            WhiskeyId = whiskey.Id,
            CurrentVolumeMl = 30, // Only a sip left
            Status = BottleStatus.Opened 
        };
        context.Bottles.Add(bottle);
        var session = new TastingSession();
        context.TastingSessions.Add(session);
        await context.SaveChangesAsync();

        var pageModel = CreateWizardModel(context);
        pageModel.SelectedBottleId = bottle.Id;
        pageModel.NewNote = new TastingNote 
        { 
            Notes = "The end.", 
            PourAmountMl = 30 // <--- Finish it
        };

        // ACT
        await pageModel.OnPostAsync(session.Id);

        // ASSERT
        var dbBottle = await context.Bottles.FindAsync(bottle.Id);
        Assert.Equal(0, dbBottle.CurrentVolumeMl);
        Assert.Equal(BottleStatus.Empty, dbBottle.Status);
    }
}