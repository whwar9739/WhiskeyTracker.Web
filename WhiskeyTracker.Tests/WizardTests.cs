using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures; // Needed for TempDataDictionary
using Microsoft.AspNetCore.Routing;
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

    [Fact]
    public async Task OnPostAsync_PopulatesWhiskeyId_WhenBottleIsSelected()
    {
        // 1. ARRANGE
        using var context = GetInMemoryContext();
        
        // Setup Fake Data
        var whiskey = new Whiskey { Id = 10, Name = "Lagavulin 16", Distillery = "Lagavulin" };
        var bottle = new Bottle { Id = 50, WhiskeyId = 10, Status = BottleStatus.Opened };
        var session = new TastingSession { Id = 1, Title = "Friday Night" };
        
        context.Whiskies.Add(whiskey);
        context.Bottles.Add(bottle);
        context.TastingSessions.Add(session);
        await context.SaveChangesAsync();

        // --- Manually initialize the PageContext ---
        var httpContext = new DefaultHttpContext();
        var modelState = new ModelStateDictionary();
        var actionContext = new ActionContext(httpContext, new RouteData(), new PageActionDescriptor(), modelState);
        var modelMetadataProvider = new EmptyModelMetadataProvider();
        var viewData = new ViewDataDictionary(modelMetadataProvider, modelState);
        var pageContext = new PageContext(actionContext)
        {
            ViewData = viewData
        };

        // --- NEW: Initialize TempData ---
        // TempData requires a "Provider" to handle storage, we use a Mock for this.
        var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

        // Initialize PageModel with the context
        var pageModel = new WizardModel(context)
        {
            PageContext = pageContext,
            TempData = tempData, // <--- ASSIGN IT HERE
            Url = new UrlHelper(actionContext), 
            
            // Simulate User Input
            SelectedBottleId = 50,
            SelectedWhiskeyId = null,
            NewNote = new TastingNote { Notes = "Smoky and great!" }
        };

        // 2. ACT
        var result = await pageModel.OnPostAsync(1);

        // 3. ASSERT
        Assert.IsType<RedirectToPageResult>(result);

        var savedNote = await context.TastingNotes.FirstOrDefaultAsync();
        Assert.NotNull(savedNote);
        Assert.Equal("Smoky and great!", savedNote.Notes);
        Assert.Equal(50, savedNote.BottleId);
        Assert.Equal(10, savedNote.WhiskeyId); 
    }
}