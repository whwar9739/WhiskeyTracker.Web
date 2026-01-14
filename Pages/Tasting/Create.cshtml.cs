using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WhiskeyTracker.Web.Data;

namespace WhiskeyTracker.Web.Pages.Tasting;

public class CreateModel : PageModel
{
    private readonly AppDbContext _context;

    public CreateModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public TastingSession Session { get; set; } = default!;

    public void OnGet()
    {
        Session = new TastingSession
        {
            Date = DateOnly.FromDateTime(DateTime.Today),
            Title = $"Tasting on {DateTime.Now:MMM dd, yyyy}"
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        _context.TastingSessions.Add(Session);
        await _context.SaveChangesAsync();

        return RedirectToPage("./Wizard", new { sessionId = Session.Id });
    }
}