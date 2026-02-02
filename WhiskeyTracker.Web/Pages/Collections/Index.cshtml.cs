using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using WhiskeyTracker.Web.Data;

namespace WhiskeyTracker.Web.Pages.Collections;

public class IndexModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public IndexModel(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public List<CollectionMember> MyMemberships { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Challenge();

        MyMemberships = await _context.CollectionMembers
            .Include(m => m.Collection)
            .ThenInclude(c => c.Bottles)
            .Include(m => m.Collection)
            .ThenInclude(c => c.Members)
            .Where(m => m.UserId == userId)
            .ToListAsync();

        return Page();
    }
}
