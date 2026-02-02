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
    public List<CollectionInvitation> MyPendingInvitations { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        MyMemberships = await _context.CollectionMembers
            .Include(m => m.Collection)
            .ThenInclude(c => c.Bottles)
            .Include(m => m.Collection)
            .ThenInclude(c => c.Members)
            .Where(m => m.UserId == user.Id)
            .ToListAsync();

        MyPendingInvitations = await _context.CollectionInvitations
            .Include(i => i.Collection)
            .Include(i => i.InviterUser)
            .Where(i => i.InviteeEmail == user.Email && i.Status == InvitationStatus.Pending)
            .ToListAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostAcceptInviteAsync(int inviteId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var invite = await _context.CollectionInvitations
            .FirstOrDefaultAsync(i => i.Id == inviteId && i.InviteeEmail == user.Email && i.Status == InvitationStatus.Pending);

        if (invite == null) return NotFound();

        invite.Status = InvitationStatus.Accepted;

        // Add user to collection
        var membership = new CollectionMember
        {
            CollectionId = invite.CollectionId,
            UserId = user.Id,
            Role = invite.Role
        };

        _context.CollectionMembers.Add(membership);
        await _context.SaveChangesAsync();

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeclineInviteAsync(int inviteId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var invite = await _context.CollectionInvitations
            .FirstOrDefaultAsync(i => i.Id == inviteId && i.InviteeEmail == user.Email && i.Status == InvitationStatus.Pending);

        if (invite == null) return NotFound();

        invite.Status = InvitationStatus.Declined;
        await _context.SaveChangesAsync();

        return RedirectToPage();
    }
}
