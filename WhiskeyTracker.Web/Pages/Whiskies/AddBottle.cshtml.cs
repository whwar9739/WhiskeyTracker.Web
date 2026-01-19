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
        
        // Use our Test-Friendly TimeProvider
        var today = DateOnly.FromDateTime(_timeProvider.GetLocalNow().DateTime);

        // Initialize Defaults
        NewBottle = new Bottle
        {
            WhiskeyId = id,
            PurchaseDate = today,
            CapacityMl = 750,      // Default standard bottle
            CurrentVolumeMl = 750  // Default to full
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Remove Whiskey validation since we set the ID manually
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