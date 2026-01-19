using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WhiskeyTracker.Web.Data;

namespace WhiskeyTracker.Web.Pages.Whiskies;

public class PourModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly TimeProvider _timeProvider;

    public PourModel(AppDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    [BindProperty]
    public int SourceBottleId { get; set;}

    [BindProperty]
    public int TargetInfinityBottleId { get; set; }

    [BindProperty]
    [Display(Name = "Amount to Pour (ml)")]
    public int PourAmountMl { get; set; }

    public Bottle SourceBottle { get; set; } = default!;
    public SelectList InfinityBottles { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var bottle = await _context.Bottles
            .Include(b => b.Whiskey)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (bottle == null) 
        {
            return NotFound();
        }

        SourceBottle = bottle;
        SourceBottleId = bottle.Id;

        PourAmountMl = bottle.CurrentVolumeMl;

        var infinityBottles = await _context.Bottles
            .Include(b => b.Whiskey)
            .Where(b => b.IsInfinityBottle && b.Id != bottle.Id)
            .ToListAsync();

        InfinityBottles = new SelectList(infinityBottles, "Id", "Whiskey.Name");

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var source = await _context.Bottles
            .Include(b => b.Whiskey)
            .FirstOrDefaultAsync(b => b.Id == SourceBottleId);

        var target = await _context.Bottles
            .Include(b => b.Whiskey)
            .FirstOrDefaultAsync(b => b.Id == TargetInfinityBottleId);

        if (source == null || target == null) 
        {
            return NotFound();
        }

        source.CurrentVolumeMl = 0;
        source.Status = BottleStatus.Empty;

        target.CurrentVolumeMl += PourAmountMl;

        var blendLog = new BlendComponent
        {
            SourceBottleId = source.Id,
            InfinityBottleId = target.Id,
            AmountAddedMl = PourAmountMl,
            DateAdded = DateOnly.FromDateTime(_timeProvider.GetLocalNow().DateTime)
        };
        _context.BlendComponents.Add(blendLog);
        await _context.SaveChangesAsync();
        return RedirectToPage("/Whiskies/Details", new { id = target.Id });
    }
}