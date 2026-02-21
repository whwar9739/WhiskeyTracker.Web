using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WhiskeyTracker.Web.Data;
using Microsoft.Extensions.Logging;

namespace WhiskeyTracker.Web.Pages.Admin;

public class BottlesModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly ILogger<BottlesModel> _logger;

    public BottlesModel(AppDbContext context, ILogger<BottlesModel> logger)
    {
        _context = context;
        _logger = logger;
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

    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public const int PageSize = 50;

    public async Task OnGetAsync(int p = 1)
    {
        CurrentPage = p;

        var totalBottles = await _context.Bottles.CountAsync();
        TotalPages = (int)Math.Ceiling(totalBottles / (double)PageSize);

        Bottles = await _context.Bottles
            .OrderBy(b => b.Whiskey?.Name)
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .Select(b => new BottleViewModel
            {
                Id = b.Id,
                WhiskeyName = b.Whiskey!.Name ?? "Unknown Whiskey",
                CollectionName = b.Collection!.Name ?? "No Collection",
                OwnerEmail = b.Purchaser!.Email ?? "No Owner",
                Status = b.Status,
                VolumePercent = b.CapacityMl > 0 ? (int)((double)b.CurrentVolumeMl / b.CapacityMl * 100) : 0
            })
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostDeleteBottleAsync(int bottleId)
    {
        var bottle = await _context.Bottles
            .Include(b => b.TastingNotes)
            .FirstOrDefaultAsync(b => b.Id == bottleId);

        if (bottle == null) return NotFound();

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Cleanup: Tasting Notes
            _context.TastingNotes.RemoveRange(bottle.TastingNotes);
            
            // Cleanup: Blend Components (where this bottle is source or target)
            var blendComponents = await _context.BlendComponents
                .Where(bc => bc.SourceBottleId == bottleId || bc.InfinityBottleId == bottleId)
                .ToListAsync();
            _context.BlendComponents.RemoveRange(blendComponents);

            _context.Bottles.Remove(bottle);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["Message"] = $"Bottle {bottleId} and its history have been deleted.";
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error deleting bottle {BottleId}", bottleId);
            TempData["ErrorMessage"] = "A critical error occurred while deleting the bottle. The operation was rolled back.";
        }

        return RedirectToPage();
    }
}

