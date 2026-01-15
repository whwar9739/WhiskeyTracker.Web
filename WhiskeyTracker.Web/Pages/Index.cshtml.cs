using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WhiskeyTracker.Web.Data;

namespace WhiskeyTracker.Web.Pages;

public class IndexModel : PageModel
{
    private readonly AppDbContext _context;


    public IndexModel(AppDbContext context)
    {
        _context = context;
    }

    public int TotalWhiskies { get; set; }
    public int OpenBottles { get; set; }
    public int TotalSessions { get; set; }

    public List<TastingNote> RecentNotes { get; set; } = new();

    public async Task OnGetAsync()
    {
        TotalWhiskies = await _context.Whiskies.CountAsync();
        OpenBottles = await _context.Bottles
            .CountAsync(b => b.Status == BottleStatus.Opened);
        TotalSessions = await _context.TastingSessions.CountAsync();

        RecentNotes = await _context.TastingNotes
            .Include(n => n.Whiskey)
            .OrderByDescending(n => n.Id)
            .Take(5)
            .ToListAsync();
    }
}
