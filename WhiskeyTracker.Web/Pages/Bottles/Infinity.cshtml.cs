using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WhiskeyTracker.Web.Data;

namespace WhiskeyTracker.Web.Pages.Bottles;

public class InfinityModel : PageModel
{
    private readonly AppDbContext _context;

    public InfinityModel(AppDbContext context)
    {
        _context = context;
    }

    public Bottle Bottle { get; set; } = default!;
    public List<BlendComponent> BlendComponents { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        var bottle = await _context.Bottles
            .Include(b => b.Whiskey)
            .Include(b => b.Collection)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (bottle == null)
        {
            return NotFound();
        }

        // Security: Check if user has access to this collection
        var canAccess = await _context.CollectionMembers
            .AnyAsync(m => m.UserId == userId && m.CollectionId == bottle.CollectionId);

        if (!canAccess)
        {
            return NotFound();
        }

        if (!bottle.IsInfinityBottle)
        {
            // Redirect back to standard details if this isn't an infinity bottle
            return RedirectToPage("/Whiskies/Details", new { id = bottle.WhiskeyId });
        }

        Bottle = bottle;

        BlendComponents = await _context.BlendComponents
            .Include(bc => bc.SourceBottle)
            .ThenInclude(sb => sb.Whiskey)
            .Where(bc => bc.InfinityBottleId == id)
            .OrderByDescending(bc => bc.DateAdded)
            .ToListAsync();

        return Page();
    }
}
