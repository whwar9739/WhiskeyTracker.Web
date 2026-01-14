using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WhiskeyTracker.Web.Data;

namespace WhiskeyTracker.Web.Pages.Whiskies;

public class DetailsModel : PageModel
{
    private readonly AppDbContext _context;

    public DetailsModel(AppDbContext context)
    {
        _context = context;
    }

    public Whiskey Whiskey { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        // fetch the whiskey AND its bottles in one go
        var whiskey = await _context.Whiskies
            .Include(w => w.Bottles) // <--- Loads the inventory data
            .FirstOrDefaultAsync(m => m.Id == id);

        if (whiskey == null) return NotFound();

        Whiskey = whiskey;
        return Page();
    }
}