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

    private IQueryable<Bottle> GetOrphanedBottles()
    {
        // Using Navigation Properties to find orphans (Foreign Key is set, but Related Entity is null)
        return _context.Bottles
            .Where(b => (b.CollectionId.HasValue && b.Collection == null) ||
                        (b.UserId != null && b.Purchaser == null));
    }

    private IQueryable<CollectionMember> GetOrphanedMembers()
    {
        return _context.CollectionMembers
            .Where(m => m.User == null || m.Collection == null);
    }

    private IQueryable<TastingNote> GetOrphanedNotes()
    {
        return _context.TastingNotes
            .Where(n => n.BottleId.HasValue && n.Bottle == null);
    }

    private IQueryable<BlendComponent> GetOrphanedBlends()
    {
        return _context.BlendComponents
            .Where(bc => bc.SourceBottle == null || bc.InfinityBottle == null);
    }

    private const int MaxScanResults = 100;

    private async Task ScanForOrphans()
    {
        Orphans.Clear();

        var badBottles = await GetOrphanedBottles().Take(MaxScanResults).ToListAsync();
        foreach (var b in badBottles)
        {
            Orphans.Add(new OrphanRecord { EntityType = "Bottle", Identifier = $"ID: {b.Id}", Reason = "Missing Collection or User" });
        }

        if (Orphans.Count >= MaxScanResults) { Orphans = Orphans.Take(MaxScanResults).ToList(); return; }

        var invalidMembers = await GetOrphanedMembers().Take(MaxScanResults - Orphans.Count).ToListAsync();
        foreach (var m in invalidMembers)
        {
            Orphans.Add(new OrphanRecord { EntityType = "CollectionMember", Identifier = $"U: {m.UserId}, C: {m.CollectionId}", Reason = "Missing User or Collection" });
        }

        if (Orphans.Count >= MaxScanResults) { Orphans = Orphans.Take(MaxScanResults).ToList(); return; }

        var invalidNotes = await GetOrphanedNotes().Take(MaxScanResults - Orphans.Count).ToListAsync();
        foreach (var n in invalidNotes)
        {
            Orphans.Add(new OrphanRecord { EntityType = "TastingNote", Identifier = $"ID: {n.Id}", Reason = "Missing Bottle" });
        }

        if (Orphans.Count >= MaxScanResults) { Orphans = Orphans.Take(MaxScanResults).ToList(); return; }

        var invalidBlends = await GetOrphanedBlends().Take(MaxScanResults - Orphans.Count).ToListAsync();
        foreach (var bc in invalidBlends)
        {
            Orphans.Add(new OrphanRecord { EntityType = "BlendComponent", Identifier = $"ID: {bc.Id}", Reason = "Missing Source or Target Bottle" });
        }

        if (Orphans.Count > MaxScanResults) { Orphans = Orphans.Take(MaxScanResults).ToList(); }
    }

    public async Task<IActionResult> OnPostCleanupAsync()
    {
        // 1. Identify Orphaned Bottles (Parents)
        var badBottles = await GetOrphanedBottles().ToListAsync();
        var badBottleIds = badBottles.Select(b => b.Id).ToList();

        // 2. Identify Dependencies of Bad Bottles (Notes/Blends that aren't natively orphaned yet, but will be)
        if (badBottleIds.Any())
        {
            var notesOfBadBottles = await _context.TastingNotes
                .Where(n => n.BottleId.HasValue && badBottleIds.Contains(n.BottleId.Value))
                .ToListAsync();
            _context.TastingNotes.RemoveRange(notesOfBadBottles);

            var blendsOfBadBottles = await _context.BlendComponents
                .Where(bc => badBottleIds.Contains(bc.SourceBottleId) || badBottleIds.Contains(bc.InfinityBottleId))
                .ToListAsync();
            _context.BlendComponents.RemoveRange(blendsOfBadBottles);
        }

        // 3. Remove Bad Bottles
        _context.Bottles.RemoveRange(badBottles);

        // 4. Remove other natively orphaned records
        var invalidMembers = await GetOrphanedMembers().ToListAsync();
        _context.CollectionMembers.RemoveRange(invalidMembers);

        var invalidNotes = await GetOrphanedNotes().ToListAsync();
        _context.TastingNotes.RemoveRange(invalidNotes);

        var invalidBlends = await GetOrphanedBlends().ToListAsync();
        _context.BlendComponents.RemoveRange(invalidBlends);

        await _context.SaveChangesAsync();
        TempData["Message"] = "Maintenance cleanup complete.";
        return RedirectToPage();
    }
}
