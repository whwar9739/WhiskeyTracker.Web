using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using WhiskeyTracker.Web.Data;

namespace WhiskeyTracker.Web.Pages.Whiskies;

public class AddBottleModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly WhiskeyTracker.Web.Services.CollectionViewModelService _collectionViewModelService;

    public AddBottleModel(AppDbContext context, TimeProvider timeProvider, UserManager<ApplicationUser> userManager, WhiskeyTracker.Web.Services.CollectionViewModelService collectionViewModelService)
    {
        _context = context;
        _timeProvider = timeProvider;
        _userManager = userManager;
        _collectionViewModelService = collectionViewModelService;
    }

    [BindProperty]
    public Bottle NewBottle { get; set; } = default!;

    public string WhiskeyName { get; set; } = string.Empty;

    public SelectList Collections { get; set; }
    public SelectList Purchasers { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var whiskey = await _context.Whiskies.FindAsync(id);
        if (whiskey == null) return NotFound();

        WhiskeyName = whiskey.Name;
        
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Challenge();

        // Populate Dropdowns via Service
        var dropdowns = await _collectionViewModelService.GetDropdownsAsync(userId);
        Collections = dropdowns.Collections;
        Purchasers = dropdowns.Purchasers;
        
        // Use our Test-Friendly TimeProvider
        var today = DateOnly.FromDateTime(_timeProvider.GetLocalNow().DateTime);

        // Initialize Defaults
        NewBottle = new Bottle
        {
            WhiskeyId = id,
            PurchaseDate = today,
            CapacityMl = 750,      // Default standard bottle
            CurrentVolumeMl = 750,  // Default to full
            UserId = userId,        // Default purchaser is me
            CollectionId = (int?)Collections.SelectedValue // Default to first collection
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Remove Whiskey validation since we set the ID manually
        ModelState.Remove("NewBottle.Whiskey");
        ModelState.Remove("NewBottle.Purchaser");
        ModelState.Remove("NewBottle.Collection");

        if (!ModelState.IsValid)
        {
            // Reload lists if validation fails
             var userId = _userManager.GetUserId(User);
             var dropdowns = await _collectionViewModelService.GetDropdownsAsync(userId, NewBottle.CollectionId, NewBottle.UserId);
             Collections = dropdowns.Collections;
             Purchasers = dropdowns.Purchasers;

             return Page();
        }

        // Verify Access to Collection
        var currentUserId = _userManager.GetUserId(User);
        var access = await _context.CollectionMembers
            .AnyAsync(m => m.UserId == currentUserId && m.CollectionId == NewBottle.CollectionId);

        if (!access)
        {
             ModelState.AddModelError("", "You do not have access to add bottles to this collection.");
             // Reload lists even on manual error
             var userId = _userManager.GetUserId(User);
             var dropdowns = await _collectionViewModelService.GetDropdownsAsync(userId, NewBottle.CollectionId, NewBottle.UserId);
             Collections = dropdowns.Collections;
             Purchasers = dropdowns.Purchasers;
             return Page();
        }

        _context.Bottles.Add(NewBottle);
        await _context.SaveChangesAsync();

        return RedirectToPage("./Details", new { id = NewBottle.WhiskeyId });
    }
}