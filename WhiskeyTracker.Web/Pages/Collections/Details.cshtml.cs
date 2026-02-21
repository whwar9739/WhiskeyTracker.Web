using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WhiskeyTracker.Web.Data;
using Microsoft.AspNetCore.Identity;

namespace WhiskeyTracker.Web.Pages.Collections;

public class DetailsModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DetailsModel(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public Collection Collection { get; set; } = null!;
    public List<CollectionMember> Members { get; set; } = new();
    public List<CollectionInvitation> PendingInvitations { get; set; } = new();
    public bool IsOwner { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Challenge();

        var collection = await _context.Collections
            .Include(c => c.Members)
            .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (collection == null) return NotFound();
        Collection = collection;

        // Check if user is a member
        var membership = Collection.Members.FirstOrDefault(m => m.UserId == userId);
        if (membership == null) return Forbid();

        IsOwner = membership.Role == CollectionRole.Owner;
        Members = Collection.Members;

        if (IsOwner)
        {
            PendingInvitations = await _context.CollectionInvitations
                .Where(i => i.CollectionId == id && i.Status == InvitationStatus.Pending)
                .ToListAsync();
        }

        return Page();
    }
}
