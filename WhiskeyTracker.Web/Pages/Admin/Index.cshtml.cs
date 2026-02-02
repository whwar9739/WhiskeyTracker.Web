using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WhiskeyTracker.Web.Data;

namespace WhiskeyTracker.Web.Pages.Admin;

public class IndexModel : PageModel
{
    private readonly AppDbContext _context;

    public IndexModel(AppDbContext context)
    {
        _context = context;
    }

    public int TotalUsers { get; set; }
    public int TotalWhiskies { get; set; }
    public int TotalBottles { get; set; }
    public int TotalCollections { get; set; }
    public int TotalTastingNotes { get; set; }

    public async Task OnGetAsync()
    {
        var usersTask = _context.Users.CountAsync();
        var whiskiesTask = _context.Whiskies.CountAsync();
        var bottlesTask = _context.Bottles.CountAsync();
        var collectionsTask = _context.Collections.CountAsync();
        var notesTask = _context.TastingNotes.CountAsync();

        await Task.WhenAll(usersTask, whiskiesTask, bottlesTask, collectionsTask, notesTask);

        TotalUsers = usersTask.Result;
        TotalWhiskies = whiskiesTask.Result;
        TotalBottles = bottlesTask.Result;
        TotalCollections = collectionsTask.Result;
        TotalTastingNotes = notesTask.Result;
    }
}
