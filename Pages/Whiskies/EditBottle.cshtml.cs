using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WhiskeyTracker.Web.Data;

namespace WhiskeyTracker.Web.Pages.Whiskies;

public class EditBottleModel : PageModel
{
    private readonly AppDbContext _context;

    public EditBottleModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Bottle Bottle { get; set; } = default!;

    public string WhiskeyName { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        var bottle = await _context.Bottles
            .Include(b => b.Whiskey)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (bottle == null) return NotFound();

        Bottle = bottle;
        WhiskeyName = bottle.Whiskey.Name;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ModelState.Remove("Bottle.Whiskey");
        if (!ModelState.IsValid)
        {
            var bottleForName = await _context.Bottles
                .Include(b => b.Whiskey)
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == Bottle.Id);

            if (bottleForName != null)
            {
                WhiskeyName = bottleForName.Whiskey.Name;
            }
            
            return Page();
        }

        _context.Attach(Bottle).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Bottles.Any(e => e.Id == Bottle.Id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return RedirectToPage("./Details", new { id = Bottle.WhiskeyId });
    }
}