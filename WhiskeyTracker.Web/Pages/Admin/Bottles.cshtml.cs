using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WhiskeyTracker.Web.Data;

namespace WhiskeyTracker.Web.Pages.Admin;

public class BottlesModel : PageModel
{
    private readonly AppDbContext _context;

    public BottlesModel(AppDbContext context)
    {
        _context = context;
    }

    public List<BottleViewModel> Bottles { get; set; } = new();

    public class BottleViewModel
    {
        public int Id { get; set; }
        public string WhiskeyName { get; set; } = string.Empty;
        public string CollectionName { get; set; } = string.Empty;
        public string OwnerEmail { get; set; } = string.Empty;
        public BottleStatus Status { get; set; }
        public int VolumePercent { get; set; }
    }

    public async Task OnGetAsync()
    {
        var bottles = await _context.Bottles
            .Include(b => b.Whiskey)
            .Include(b => b.Collection)
            .Include(b => b.Purchaser)
            .ToListAsync();

        foreach (var b in bottles)
        {
            Bottles.Add(new BottleViewModel
            {
                Id = b.Id,
                WhiskeyName = b.Whiskey?.Name ?? "Unknown Whiskey",
                CollectionName = b.Collection?.Name ?? "No Collection",
                OwnerEmail = b.Purchaser?.Email ?? "No Owner",
                Status = b.Status,
                VolumePercent = b.CapacityMl > 0 ? (int)((double)b.CurrentVolumeMl / b.CapacityMl * 100) : 0
            });
        }
    }

    public async Task<IActionResult> OnPostDeleteBottleAsync(int bottleId)
    {
        var bottle = await _context.Bottles
            .Include(b => b.TastingNotes)
            .FirstOrDefaultAsync(b => b.Id == bottleId);

        if (bottle == null) return NotFound();

        // Cleanup: Tasting Notes
        _context.TastingNotes.RemoveRange(bottle.TastingNotes);
        
        // Cleanup: Blend Components (where this bottle is source or target)
        var blendComponents = await _context.BlendComponents
            .Where(bc => bc.SourceBottleId == bottleId || bc.InfinityBottleId == bottleId)
            .ToListAsync();
        _context.BlendComponents.RemoveRange(blendComponents);

        _context.Bottles.Remove(bottle);
        await _context.SaveChangesAsync();

        TempData["Message"] = $"Bottle {bottleId} and its history have been deleted.";
        return RedirectToPage();
    }
}
