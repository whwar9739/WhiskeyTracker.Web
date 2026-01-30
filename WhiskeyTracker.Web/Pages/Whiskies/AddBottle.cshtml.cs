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

    public AddBottleModel(AppDbContext context, TimeProvider timeProvider, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _timeProvider = timeProvider;
        _userManager = userManager;
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

        // 1. Get My Collections
        var myMemberships = await _context.CollectionMembers
            .Include(m => m.Collection)
            .Where(m => m.UserId == userId)
            .ToListAsync();
            
        Collections = new SelectList(myMemberships.Select(m => m.Collection), "Id", "Name");

        // 2. Get Potential Purchasers (All members of my collections)
        // Note: For now, we'll fetch all unique users involved in my collections
        var myCollectionIds = myMemberships.Select(m => m.CollectionId).ToList();

        var allMemberUserIds = await _context.CollectionMembers
            .Where(m => myCollectionIds.Contains(m.CollectionId))
            .Select(m => m.UserId)
            .Distinct()
            .ToListAsync();

        var users = await _userManager.Users
            .Where(u => allMemberUserIds.Contains(u.Id))
            .ToListAsync();
        
        // Format names nicely
        var purchaserList = users.Select(u => new 
        { 
            Id = u.Id, 
            Name = string.IsNullOrEmpty(u.DisplayName) ? u.UserName : u.DisplayName 
        }).ToList();

        Purchasers = new SelectList(purchaserList, "Id", "Name", userId);

        
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
            CollectionId = myMemberships.FirstOrDefault()?.CollectionId // Default to first collection
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
             var myMemberships = await _context.CollectionMembers
                .Include(m => m.Collection)
                .Where(m => m.UserId == userId)
                .ToListAsync();
            Collections = new SelectList(myMemberships.Select(m => m.Collection), "Id", "Name");
            
            var myCollectionIds = myMemberships.Select(m => m.CollectionId).ToList();
             var allMemberUserIds = await _context.CollectionMembers
                .Where(m => myCollectionIds.Contains(m.CollectionId))
                .Select(m => m.UserId)
                .Distinct()
                .ToListAsync();
            var users = await _userManager.Users
                .Where(u => allMemberUserIds.Contains(u.Id))
                .ToListAsync();
            var purchaserList = users.Select(u => new 
            { 
                Id = u.Id, 
                Name = string.IsNullOrEmpty(u.DisplayName) ? u.UserName : u.DisplayName 
            }).ToList();
            Purchasers = new SelectList(purchaserList, "Id", "Name", NewBottle.UserId);

            return Page();
        }

        // Verify Access to Collection
        var currentUserId = _userManager.GetUserId(User);
        var access = await _context.CollectionMembers
            .AnyAsync(m => m.UserId == currentUserId && m.CollectionId == NewBottle.CollectionId);

        if (!access)
        {
             ModelState.AddModelError("", "You do not have access to add bottles to this collection.");
             return Page();
        }

        _context.Bottles.Add(NewBottle);
        await _context.SaveChangesAsync();

        return RedirectToPage("./Details", new { id = NewBottle.WhiskeyId });
    }
}