using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Moq;
using WhiskeyTracker.Web.Data;
using WhiskeyTracker.Web.Pages.Whiskies;
using Xunit;

namespace WhiskeyTracker.Tests;

public class WhiskiesTests
{
    private AppDbContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    // --- INDEX TESTS ---
    [Fact]
    public async Task Index_ReturnsAllWhiskies_WhenNoSearch()
    {
        using var context = GetInMemoryContext();
        context.Whiskies.Add(new Whiskey { Name = "Ardbeg", Region = "Islay" });
        context.Whiskies.Add(new Whiskey { Name = "Macallan", Region = "Speyside" });
        await context.SaveChangesAsync();

        var pageModel = new IndexModel(context);

        await pageModel.OnGetAsync();

        Assert.Equal(2, pageModel.Whiskies.Count);
        Assert.Contains(pageModel.Whiskies, w => w.Name == "Ardbeg");
    }

    [Fact]
    public async Task Index_FiltersBySearchString()
    {
        using var context = GetInMemoryContext();
        context.Whiskies.Add(new Whiskey { Name = "Ardbeg", Distillery = "Ardbeg" });
        context.Whiskies.Add(new Whiskey { Name = "Balvenie", Distillery = "Balvenie" });
        await context.SaveChangesAsync();

        var pageModel = new IndexModel(context) { SearchString = "Ard" };

        await pageModel.OnGetAsync();

        Assert.Single(pageModel.Whiskies);
        Assert.Equal("Ardbeg", pageModel.Whiskies[0].Name);
    }

    [Fact]
    public async Task Index_FiltersByRegion()
    {
        using var context = GetInMemoryContext();
        context.Whiskies.Add(new Whiskey { Name = "A", Region = "Islay" });
        context.Whiskies.Add(new Whiskey { Name = "B", Region = "Speyside" });
        await context.SaveChangesAsync();

        var pageModel = new IndexModel(context) { WhiskeyRegion = "Islay" };

        await pageModel.OnGetAsync();

        Assert.Single(pageModel.Whiskies);
        Assert.Equal("A", pageModel.Whiskies[0].Name);
    }

    // --- CREATE TESTS (With File Upload) ---
    [Fact]
    public async Task Create_UploadsFile_AndRedirects()
    {
        using var context = GetInMemoryContext();

        // Mock the Environment to use a temp folder
        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(m => m.WebRootPath).Returns(Path.GetTempPath());

        // Mock the File Upload
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("test.jpg");
        mockFile.Setup(f => f.Length).Returns(1024);
        // We mock CopyToAsync to verify it was called
        mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

        var pageModel = new CreateModel(context, mockEnv.Object)
        {
            NewWhiskey = new Whiskey { Name = "Test Whiskey" },
            ImageUpload = mockFile.Object
        };

        var result = await pageModel.OnPostAsync();

        mockFile.Verify(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.IsType<RedirectToPageResult>(result);
        var whiskey = await context.Whiskies.FirstAsync();
        Assert.Equal("Test Whiskey", whiskey.Name);
        Assert.Contains("test.jpg", whiskey.ImageFileName); // Confirms filename was generated
    }

    // --- DETAILS TESTS ---
    [Fact]
    public async Task Details_ReturnsNotFound_WhenIdIsNull()
    {
        using var context = GetInMemoryContext();
        var pageModel = new DetailsModel(context);
        var result = await pageModel.OnGetAsync(null);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_PopulatesData_WhenFound()
    {
        using var context = GetInMemoryContext();
        context.Whiskies.Add(new Whiskey { Id = 1, Name = "Test" });
        await context.SaveChangesAsync();

        var pageModel = new DetailsModel(context);
        await pageModel.OnGetAsync(1);

        Assert.NotNull(pageModel.Whiskey);
        Assert.Equal("Test", pageModel.Whiskey.Name);
    }

    // --- DELETE TESTS ---
    [Fact]
    public async Task Delete_RemovesWhiskey_WhenFound()
    {
        using var context = GetInMemoryContext();
        context.Whiskies.Add(new Whiskey { Id = 1, Name = "To Delete" });
        await context.SaveChangesAsync();

        var pageModel = new DeleteModel(context);
        var result = await pageModel.OnPostAsync(1);

        Assert.IsType<RedirectToPageResult>(result);
        Assert.Empty(context.Whiskies);
    }
    [Fact]
    public async Task Delete_ReturnsNotFound_WhenWhiskeyNotFound()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var pageModel = new DeleteModel(context);

        // Act
        var result = await pageModel.OnPostAsync(999); // A non-existent ID

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}