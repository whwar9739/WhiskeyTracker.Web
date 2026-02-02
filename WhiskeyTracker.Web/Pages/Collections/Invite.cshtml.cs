using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WhiskeyTracker.Web.Data;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace WhiskeyTracker.Web.Pages.Collections;

public class InviteModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public InviteModel(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string CollectionName { get; set; } = string.Empty;

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public CollectionRole Role { get; set; } = CollectionRole.Viewer;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Challenge();

        var collection = await _context.Collections
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (collection == null) return NotFound();

        // Check if user is owner
        var membership = collection.Members.FirstOrDefault(m => m.UserId == userId);
        if (membership == null || membership.Role != CollectionRole.Owner) return Forbid();

        CollectionName = collection.Name;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Challenge();

        var collection = await _context.Collections
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (collection == null) return NotFound();

        var membership = collection.Members.FirstOrDefault(m => m.UserId == userId);
        if (membership == null || membership.Role != CollectionRole.Owner) return Forbid();

        if (!ModelState.IsValid)
        {
            CollectionName = collection.Name;
            return Page();
        }

        // Check if invitee is already a member
        var invitee = await _userManager.FindByEmailAsync(Input.Email);
        if (invitee != null && collection.Members.Any(m => m.UserId == invitee.Id))
        {
            ModelState.AddModelError("Input.Email", "This user is already a member of the collection.");
            CollectionName = collection.Name;
            return Page();
        }

        // Check if there is already a pending invitation
        var existingInvite = await _context.CollectionInvitations
            .FirstOrDefaultAsync(i => i.CollectionId == id && i.InviteeEmail == Input.Email && i.Status == InvitationStatus.Pending);

        if (existingInvite != null)
        {
            ModelState.AddModelError("Input.Email", "There is already a pending invitation for this email.");
            CollectionName = collection.Name;
            return Page();
        }

        var invitation = new CollectionInvitation
        {
            CollectionId = id,
            InviterUserId = userId,
            InviteeEmail = Input.Email,
            Role = Input.Role,
            Status = InvitationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.CollectionInvitations.Add(invitation);
        await _context.SaveChangesAsync();

        return RedirectToPage("./Details", new { id });
    }
}
