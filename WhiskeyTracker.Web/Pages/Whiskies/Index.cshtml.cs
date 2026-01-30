using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WhiskeyTracker.Web.Data;

namespace WhiskeyTracker.Web.Pages.Whiskies;

public class IndexModel : PageModel
{
    private readonly AppDbContext _context;

    public IndexModel(AppDbContext context)
    {
        _context = context;
    }

    // This list will hold the data we fetch so the HTML can see it
    public IList<Whiskey> Whiskies { get; set; } = default!;

    [BindProperty(SupportsGet = true)]
    public string? SearchString { get; set; }
    public SelectList? Regions { get; set; }
    [BindProperty(SupportsGet = true)]
    public string? WhiskeyRegion { get; set; }

    public async Task OnGetAsync()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        // --- 1. Runtime Migration: Ensure User has a Collection ---
        if (!string.IsNullOrEmpty(userId))
        {
            var hasCollection = await _context.CollectionMembers.AnyAsync(m => m.UserId == userId);
            if (!hasCollection)
            {
                // Create Default Collection
                var personalCollection = new Collection { Name = "My Home Bar" };
                _context.Collections.Add(personalCollection);
                await _context.SaveChangesAsync();

                _context.CollectionMembers.Add(new CollectionMember
                {
                    CollectionId = personalCollection.Id,
                    UserId = userId,
                    Role = CollectionRole.Owner
                });
                await _context.SaveChangesAsync();
            }

            // --- 2. Runtime Migration: Adopt Orphan Bottles ---
            var orphanBottles = await _context.Bottles
                .Where(b => b.UserId == userId && b.CollectionId == null)
                .ToListAsync();

            if (orphanBottles.Any())
            {
                // Assign to their first collection
                // Assign to their first collection
                var member = await _context.CollectionMembers
                    .Where(m => m.UserId == userId)
                    .OrderBy(m => m.Id)
                    .FirstOrDefaultAsync();
                var myCollectionId = member?.CollectionId ?? 0;

                foreach (var orphan in orphanBottles)
                {
                    orphan.CollectionId = myCollectionId;
                }
                await _context.SaveChangesAsync();
            }
        }
        
        IQueryable<string> genreQuery = _context.Whiskies
                                        .OrderBy(w => w.Region)
                                        .Select(w => w.Region)
                                        .Distinct();
        Regions = new SelectList(await genreQuery.ToListAsync());

        // Show Whiskies that exist in the DB.
        // Option: Filter to only show whiskies I have? No, library mode usually shows all reference data.
        var whiskies = from w in _context.Whiskies
                       select w;

        if (!string.IsNullOrEmpty(SearchString))
        {
            whiskies = whiskies.Where(s => s.Name.ToLower().Contains(SearchString.ToLower())
                                        || s.Distillery.ToLower().Contains(SearchString.ToLower()));
        }

        if (!string.IsNullOrEmpty(WhiskeyRegion))
        {
            whiskies = whiskies.Where(x => x.Region == WhiskeyRegion);
        }

        Whiskies = await whiskies.ToListAsync();
    }
}