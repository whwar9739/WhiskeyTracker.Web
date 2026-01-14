using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WhiskeyTracker.Web.Data;

namespace WhiskeyTracker.Web.Pages.Tasting;

public class WizardModel : PageModel
{
    private readonly AppDbContext _context;

    public WizardModel(AppDbContext context)
    {
        _context = context;
    }

    public TastingSession Session { get; set; } = default!;
    
    [BindProperty]
    public TastingNote NewNote { get; set; } = default!;
    
    public SelectList WhiskeyOptions { get; set; } = default!;

    // GET: Prepares the page for viewing
    public async Task<IActionResult> OnGetAsync(int sessionId)
    {
        var session = await _context.TastingSessions
            .Include(s => s.Notes)
            .ThenInclude(n => n.Whiskey) // Include Whiskey so we can see names in the list
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        if (session == null) return NotFound();

        Session = session;

        // FIX: Initialize NewNote so it is not null
        NewNote = new TastingNote 
        { 
            TastingSessionId = sessionId,
            OrderIndex = session.Notes.Count + 1 
        };

        // Load the dropdown
        var availableWhiskies = _context.Whiskies
            .Where(w => w.Bottles.Any(b => b.Status != BottleStatus.Empty))
            .OrderBy(w => w.Name);
        WhiskeyOptions = new SelectList(availableWhiskies, "Id", "Name");

        return Page();
    }

    // POST: Handles the "Log & Pour Next" button
    public async Task<IActionResult> OnPostAsync(int sessionId)
    {
        var session = await _context.TastingSessions
            .Include(s => s.Notes)
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        if (session == null) return NotFound();

        // Re-establish relationships
        NewNote.TastingSessionId = sessionId;
        NewNote.OrderIndex = session.Notes.Count + 1;

        // Validation exclusions
        ModelState.Remove("NewNote.TastingSession");
        ModelState.Remove("NewNote.Whiskey");

        if (!ModelState.IsValid)
        {
            // If invalid, reload the page data so the user can fix errors
            Session = session;
            var availableWhiskies = _context.Whiskies
                .Where(w => w.Bottles.Any(b => b.Status != BottleStatus.Empty))
                .OrderBy(w => w.Name);
            WhiskeyOptions = new SelectList(availableWhiskies, "Id", "Name");
            return Page();
        }

        _context.TastingNotes.Add(NewNote);
        await _context.SaveChangesAsync();

        // Redirect to the SAME page to refresh the list and clear the form
        return RedirectToPage(new { sessionId = sessionId });
    }
}