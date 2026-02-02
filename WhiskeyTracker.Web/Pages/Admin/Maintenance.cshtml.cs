using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WhiskeyTracker.Web.Data;

namespace WhiskeyTracker.Web.Pages.Admin;

public class MaintenanceModel : PageModel
{
    private readonly AppDbContext _context;

    public MaintenanceModel(AppDbContext context)
    {
        _context = context;
    }

    public List<OrphanRecord> Orphans { get; set; } = new();

    public class OrphanRecord
    {
        public string EntityType { get; set; } = string.Empty;
        public string Identifier { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }

    public async Task OnGetAsync()
    {
        await ScanForOrphans();
    }

    private async Task ScanForOrphans()
    {
        Orphans.Clear();

        // 1. Bottles without valid Collection
        var collectionsIds = await _context.Collections.Select(c => c.Id).ToListAsync();
        var bottlesWithMissingCollections = await _context.Bottles
            .Where(b => b.CollectionId.HasValue && !collectionsIds.Contains(b.CollectionId.Value))
            .ToListAsync();
        
        foreach (var b in bottlesWithMissingCollections)
        {
            Orphans.Add(new OrphanRecord { EntityType = "Bottle", Identifier = $"ID: {b.Id}", Reason = "Missing Collection" });
        }

        // 2. Bottles without valid User
        var userIds = await _context.Users.Select(u => u.Id).ToListAsync();
        var bottlesWithMissingUsers = await _context.Bottles
            .Where(b => !string.IsNullOrEmpty(b.UserId) && !userIds.Contains(b.UserId))
            .ToListAsync();
        
        foreach (var b in bottlesWithMissingUsers)
        {
            Orphans.Add(new OrphanRecord { EntityType = "Bottle", Identifier = $"ID: {b.Id}", Reason = "Missing User" });
        }

        // 3. CollectionMembers with missing User or Collection
        var invalidMembers = await _context.CollectionMembers
            .Where(m => !userIds.Contains(m.UserId) || !collectionsIds.Contains(m.CollectionId))
            .ToListAsync();
        
        foreach (var m in invalidMembers)
        {
            Orphans.Add(new OrphanRecord { EntityType = "CollectionMember", Identifier = $"U: {m.UserId}, C: {m.CollectionId}", Reason = "Missing User or Collection" });
        }

        // 4. TastingNotes without valid Bottle
        var bottleIds = await _context.Bottles.Select(b => b.Id).ToListAsync();
        var invalidNotes = await _context.TastingNotes
            .Where(n => n.BottleId.HasValue && !bottleIds.Contains(n.BottleId.Value))
            .ToListAsync();
            
        foreach (var n in invalidNotes)
        {
            Orphans.Add(new OrphanRecord { EntityType = "TastingNote", Identifier = $"ID: {n.Id}", Reason = "Missing Bottle" });
        }
        
        // 5. BlendComponents without valid Source or Infinity Bottle
        var invalidBlends = await _context.BlendComponents
            .Where(bc => !bottleIds.Contains(bc.SourceBottleId) || !bottleIds.Contains(bc.InfinityBottleId))
            .ToListAsync();
            
        foreach (var bc in invalidBlends)
        {
            Orphans.Add(new OrphanRecord { EntityType = "BlendComponent", Identifier = $"ID: {bc.Id}", Reason = "Missing Source or Target Bottle" });
        }
    }

    public async Task<IActionResult> OnPostCleanupAsync()
    {
        var collectionsIds = await _context.Collections.Select(c => c.Id).ToListAsync();
        var userIds = await _context.Users.Select(u => u.Id).ToListAsync();
        
        // 1. Bottles without valid Collection/User
        var badBottles = await _context.Bottles
            .Where(b => (b.CollectionId.HasValue && !collectionsIds.Contains(b.CollectionId.Value)) || 
                        (!string.IsNullOrEmpty(b.UserId) && !userIds.Contains(b.UserId)))
            .ToListAsync();
        _context.Bottles.RemoveRange(badBottles);

        // 2. Invalid Memberships
        var invalidMembers = await _context.CollectionMembers
            .Where(m => !userIds.Contains(m.UserId) || !collectionsIds.Contains(m.CollectionId))
            .ToListAsync();
        _context.CollectionMembers.RemoveRange(invalidMembers);

        // 3. Invalid Notes
        var bottleIds = await _context.Bottles.Select(b => b.Id).ToListAsync();
        var invalidNotes = await _context.TastingNotes
            .Where(n => n.BottleId.HasValue && !bottleIds.Contains(n.BottleId.Value))
            .ToListAsync();
        _context.TastingNotes.RemoveRange(invalidNotes);

        // 4. Invalid Blends
        var invalidBlends = await _context.BlendComponents
            .Where(bc => !bottleIds.Contains(bc.SourceBottleId) || !bottleIds.Contains(bc.InfinityBottleId))
            .ToListAsync();
        _context.BlendComponents.RemoveRange(invalidBlends);

        await _context.SaveChangesAsync();
        TempData["Message"] = "Maintenance cleanup complete.";
        return RedirectToPage();
    }
}
