using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.VisualBasic;
using WhiskeyTracker.Web.Data;

namespace WhiskeyTracker.Web.Pages.Tasting;

public class WizardModel : PageModel
{
    private readonly AppDbContext _context;

    public WizardModel(AppDbContext context)
    {
        _context = context;
    }

    public TastingSession Session { get; set; } = new();
    
    [BindProperty]
    public TastingNote NewNote { get; set; } = new();

    [BindProperty]
    public int? SelectedBottleId { get; set; }
    [BindProperty]
    public int? SelectedWhiskeyId { get; set; }
    
    public SelectList BottleOptions { get; set; } = default!;
    public SelectList WhiskeyOptions { get; set; } = default!;

    // GET: Prepares the page for viewing
    public async Task<IActionResult> OnGetAsync(int sessionId)
    {

        await LoadPageData(sessionId);

        if (Session.Id == 0)
        {
            return NotFound($"Session with ID {sessionId} not found.");
        }

        return Page();
    }

    // POST: Handles the "Log & Pour Next" button
    public async Task<IActionResult> OnPostAsync(int sessionId)
    {
        var sessionExists = await _context.TastingSessions.AnyAsync(s => s.Id == sessionId);
        if (!sessionExists) return NotFound();

        if (SelectedBottleId.HasValue)
        {
            var bottle = await _context.Bottles.FindAsync(SelectedBottleId.Value);
            if (bottle != null)
            {
                NewNote.WhiskeyId = bottle.WhiskeyId;
                NewNote.BottleId = bottle.Id;
            }
        }
        else if (SelectedWhiskeyId.HasValue)
        {
            NewNote.WhiskeyId = SelectedWhiskeyId.Value;
            NewNote.BottleId = null;
        }
        else
        {
            ModelState.AddModelError("SelectedBottleId", "You must select either a Bottle or a Whiskey.");
            ModelState.AddModelError("SelectedWhiskeyId", "You must select either a Bottle or a Whiskey.");
        }

        if (string.IsNullOrWhiteSpace(NewNote.Notes))
        {
            ModelState.AddModelError("NewNote.Notes", "Tasting notes are required.");
        }

        // Re-establish relationships
        NewNote.TastingSessionId = sessionId;
        NewNote.OrderIndex = await _context.TastingNotes.CountAsync(n => n.TastingSessionId == sessionId) + 1;

        // Validation exclusions
        ModelState.Remove("NewNote.TastingSession");
        ModelState.Remove("NewNote.Whiskey");
        ModelState.Remove("NewNote.Bottle");

        if (!ModelState.IsValid)
        {
            await LoadPageData(sessionId);
            return Page();
        }

        _context.TastingNotes.Add(NewNote);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Tasting note added successfully! Ready for the next pour!";

        // Redirect to the SAME page to refresh the list and clear the form
        return RedirectToPage(new { sessionId });
    }

    private async Task LoadPageData(int sessionId)
    {
        var session = await _context.TastingSessions
            .Include(s => s.Notes)
            .ThenInclude(n => n.Whiskey) // Include Whiskey so we can see names in the list
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        if (session != null)
        {
            Session = session;
        }

        // FIX: Initialize NewNote so it is not null
        NewNote = new TastingNote();

        var bottles = await _context.Bottles
            .Include(b => b.Whiskey)
            .Where(b => b.Status != BottleStatus.Empty)
            .OrderBy(b => b.Whiskey.Name)
            .ToListAsync();

        BottleOptions = new SelectList(bottles.Select(b => new {
            b.Id,
            Text = $"{b.Whiskey.Name} ({b.Status})"
        }), "Id", "Text");

        var whiskies = await _context.Whiskies
            .OrderBy(w => w.Name)
            .Select(w => new { w.Id, w.Name })
            .ToListAsync();

        WhiskeyOptions = new SelectList(whiskies, "Id", "Name");
    }
}