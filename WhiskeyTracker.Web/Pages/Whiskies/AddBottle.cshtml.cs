using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WhiskeyTracker.Web.Data;

namespace WhiskeyTracker.Web.Pages.Whiskies;

public class AddBottleModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly TimeProvider _timeProvider;

    public AddBottleModel(AppDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    [BindProperty]
    public Bottle NewBottle { get; set; } = default!;

    public string WhiskeyName { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var whiskey = await _context.Whiskies.FindAsync(id);
        if (whiskey == null) return NotFound();

        WhiskeyName = whiskey.Name;
        var today = DateOnly.FromDateTime(_timeProvider.GetLocalNow().Date);

        NewBottle = new Bottle
        {
            WhiskeyId = id,
            PurchaseDate = today
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