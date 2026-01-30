using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using WhiskeyTracker.Web.Data;

namespace WhiskeyTracker.Web.Pages.Whiskies;

public class EditBottleModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public EditBottleModel(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
        Collections = new SelectList(new List<Collection>(), "Id", "Name");
        Purchasers = new SelectList(new List<object>(), "Id", "Name");
    }

    [BindProperty]
    public Bottle Bottle { get; set; } = default!;

    public string WhiskeyName { get; set; } = string.Empty;
    public SelectList Collections { get; set; }
    public SelectList Purchasers { get; set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        var userId = _userManager.GetUserId(User);

        var bottle = await _context.Bottles
            .Include(b => b.Whiskey)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (bottle == null) return NotFound();

        // 1. Get My Collections
        var myMemberships = await _context.CollectionMembers
            .Include(m => m.Collection)
            .Where(m => m.UserId == userId)
            .ToListAsync();
        
        // Security: Check Collection Access (Must be in valid collection currently)
        var canAccess = myMemberships.Any(m => m.CollectionId == bottle.CollectionId);
        if (!canAccess) return NotFound();

        // Populate Lists
        Collections = new SelectList(myMemberships.Select(m => m.Collection), "Id", "Name", bottle.CollectionId);

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

        Purchasers = new SelectList(purchaserList, "Id", "Name", bottle.UserId);

        Bottle = bottle;
        if (bottle.Whiskey != null)
            WhiskeyName = bottle.Whiskey.Name;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ModelState.Remove("Bottle.Whiskey");
        ModelState.Remove("Bottle.Purchaser");
        ModelState.Remove("Bottle.Collection");

        if (!ModelState.IsValid)
        {
            // Reload Lists
             var userId = _userManager.GetUserId(User);
             var myMemberships = await _context.CollectionMembers
                .Include(m => m.Collection)
                .Where(m => m.UserId == userId)
                .ToListAsync();
            Collections = new SelectList(myMemberships.Select(m => m.Collection), "Id", "Name", Bottle.CollectionId);
            
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
            Purchasers = new SelectList(purchaserList, "Id", "Name", Bottle.UserId);
            
            return Page();
        }

        var currentUserId = _userManager.GetUserId(User);

        // Fetch original to check access
        var originalBottle = await _context.Bottles.AsNoTracking().FirstOrDefaultAsync(b => b.Id == Bottle.Id);
        if (originalBottle == null) return NotFound();

        // 1. Check access to ORIGINAL collection
        var canAccessOriginal = await _context.CollectionMembers.AnyAsync(m => m.UserId == currentUserId && m.CollectionId == originalBottle.CollectionId);
        if (!canAccessOriginal) return NotFound();

        // 2. Check access to NEW collection (if changed)
        if (originalBottle.CollectionId != Bottle.CollectionId)
        {
             var canAccessNew = await _context.CollectionMembers.AnyAsync(m => m.UserId == currentUserId && m.CollectionId == Bottle.CollectionId);
             if (!canAccessNew)
             {
                 ModelState.AddModelError("", "You do not have access to move the bottle to this collection.");
                 return Page();
             }
        }

        _context.Attach(Bottle).State = EntityState.Modified;
        
        // Prevent accidental property modification if needed, but here we bind what we want.

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