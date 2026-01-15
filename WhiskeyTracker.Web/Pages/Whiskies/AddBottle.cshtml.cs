using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WhiskeyTracker.Web.Data;

namespace WhiskeyTracker.Web.Pages.Whiskies;

public class AddBottleModel : PageModel
{
    private readonly AppDbContext _context;

    public AddBottleModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Bottle NewBottle { get; set; } = default!;

    public string WhiskeyName { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var whiskey = await _context.Whiskies.FindAsync(id);
        if (whiskey == null) return NotFound();

        WhiskeyName = whiskey.Name;

        NewBottle = new Bottle
        {
            WhiskeyId = id,
            PurchaseDate = DateOnly.FromDateTime(DateTime.Today)
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ModelState.Remove("NewBottle.Whiskey");

        if (!ModelState.IsValid)
        {
            return Page();
        }

        _context.Bottles.Add(NewBottle);
        await _context.SaveChangesAsync();

        return RedirectToPage("./Details", new { id = NewBottle.WhiskeyId });
    }
}