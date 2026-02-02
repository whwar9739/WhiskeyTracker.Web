using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WhiskeyTracker.Web.Data;
using Microsoft.Extensions.Logging;

namespace WhiskeyTracker.Web.Pages.Admin;

public class MaintenanceModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly ILogger<MaintenanceModel> _logger;

    public MaintenanceModel(AppDbContext context, ILogger<MaintenanceModel> logger)
    {
        _context = context;
        _logger = logger;
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

        // Refactored to reduce duplication
        await AddOrphansAsync("Bottle", GetOrphanedBottles(), b => $"ID: {b.Id}", "Missing Collection or User");
        await AddOrphansAsync("CollectionMember", GetOrphanedMembers(), m => $"U: {m.UserId}, C: {m.CollectionId}", "Missing User or Collection");
        await AddOrphansAsync("TastingNote", GetOrphanedNotes(), n => $"ID: {n.Id}", "Missing Bottle");
        await AddOrphansAsync("BlendComponent", GetOrphanedBlends(), bc => $"ID: {bc.Id}", "Missing Source or Target Bottle");

        if (Orphans.Count > MaxScanResults)
        {
            Orphans = Orphans.Take(MaxScanResults).ToList();
        }
    }

    private async Task AddOrphansAsync<T>(string type, IQueryable<T> query, Func<T, string> idSelector, string reason) where T : class
    {
        int remaining = MaxScanResults - Orphans.Count;
        if (remaining <= 0) return;

        var items = await query.Take(remaining).ToListAsync();
        foreach (var item in items)
        {
            Orphans.Add(new OrphanRecord 
            { 
                EntityType = type, 
                Identifier = idSelector(item), 
                Reason = reason 
            });
        }
    }

    public async Task<IActionResult> OnPostCleanupAsync()
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. Identify Orphaned Bottles (Parents)
            var badBottles = await GetOrphanedBottles().ToListAsync();
            var badBottleIds = badBottles.Select(b => b.Id).ToList();

            // 2. Identify Dependencies (Notes/Blends)
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
            await transaction.CommitAsync();

            TempData["Message"] = "Maintenance cleanup complete.";
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "A critical error occurred while performing maintenance cleanup.");
            TempData["ErrorMessage"] = "A critical error occurred during cleanup. The operation was rolled back.";
        }

        return RedirectToPage();
    }
}
