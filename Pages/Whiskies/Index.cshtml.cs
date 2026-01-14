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
        IQueryable<string> genreQuery = _context.Whiskies
                                        .OrderBy(w => w.Region)
                                        .Select(w => w.Region)
                                        .Distinct();
        Regions = new SelectList(await genreQuery.ToListAsync());

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