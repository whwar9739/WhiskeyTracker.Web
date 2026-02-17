using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WhiskeyTracker.Web.Data;

namespace WhiskeyTracker.Web.Pages.Tasting;

public class IndexModel : PageModel
{
    private readonly AppDbContext _context;

    public IndexModel(AppDbContext context)
    {
        _context = context;
    }

    public IList<TastingSessionViewModel> Sessions { get;set; } = default!;

    public async Task OnGetAsync()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
        {
            Sessions = new List<TastingSessionViewModel>();
            return;
        }

        var sessions = await _context.TastingSessions
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.Date)
            .ThenByDescending(s => s.Id)
            .Select(s => new TastingSessionViewModel
            {
                Id = s.Id,
                Date = s.Date,
                Title = s.Title,
                NotesCount = s.Notes.Count
            })
            .ToListAsync();

        Sessions = sessions;
    }

    public class TastingSessionViewModel
    {
        public int Id { get; set; }
        public DateOnly Date { get; set; }
        public string Title { get; set; }
        public int NotesCount { get; set; }
    }
}
