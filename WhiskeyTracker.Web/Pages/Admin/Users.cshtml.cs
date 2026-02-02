using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WhiskeyTracker.Web.Data;

namespace WhiskeyTracker.Web.Pages.Admin;

public class UsersModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AppDbContext _context;

    public UsersModel(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, AppDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    public List<UserViewModel> Users { get; set; } = new();

    public class UserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? DisplayName { get; set; }
        public bool IsAdmin { get; set; }
    }

    public async Task OnGetAsync()
    {
        // Resolve N+1 issue: Fetch all users in Admin role first
        // 1. Get the admin role ID
        var adminRole = await _roleManager.FindByNameAsync("Admin");
        var adminRoleId = adminRole?.Id;

        // 2. Get all UserIDs that have this role
        var adminUserIds = new HashSet<string>();
        if (adminRoleId != null)
        {
            var adminUserIdsList = await _context.UserRoles
                .Where(ur => ur.RoleId == adminRoleId)
                .Select(ur => ur.UserId)
                .ToListAsync();
            adminUserIds = new HashSet<string>(adminUserIdsList);
        }

        // 3. Fetch users and map, checking against the HashSet
        Users = await _context.Users
            .Select(u => new UserViewModel
            {
                Id = u.Id,
                Email = u.Email,
                DisplayName = u.DisplayName,
                IsAdmin = false // Placeholder, set in memory below to avoid complexity in LINQ translation if not needed
            })
            .ToListAsync();

        foreach (var u in Users)
        {
            u.IsAdmin = adminUserIds.Contains(u.Id);
        }
    }

    public async Task<IActionResult> OnPostToggleAdminAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        // Prevent self-demotion if the user is the one performing the action (safeguard)
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser?.Id == userId)
        {
            TempData["ErrorMessage"] = "You cannot demote yourself.";
            return RedirectToPage();
        }

        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
        if (isAdmin)
        {
            await _userManager.RemoveFromRoleAsync(user, "Admin");
            TempData["Message"] = $"Admin role removed from {user.Email}.";
        }
        else
        {
            await _userManager.AddToRoleAsync(user, "Admin");
            TempData["Message"] = $"Admin role added to {user.Email}.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser?.Id == userId)
        {
            TempData["ErrorMessage"] = "You cannot delete yourself.";
            return RedirectToPage();
        }

        // Cleanup: Tasting Sessions & Notes
        var sessions = await _context.TastingSessions.Where(s => s.UserId == userId).ToListAsync();
        _context.TastingSessions.RemoveRange(sessions);

        // Optimizing cleanup:
        // We need to delete notes that are either BY the user OR attached to bottles OF the user.
        
        // 1. Delete all notes BY the user
        var userNotes = await _context.TastingNotes.Where(n => n.UserId == userId).ToListAsync();
        _context.TastingNotes.RemoveRange(userNotes);

        // 2. Fetch bottles owned by user
        var bottles = await _context.Bottles.Where(b => b.UserId == userId).ToListAsync();
        var bottleIds = bottles.Select(b => b.Id).ToList();

        // 3. Cleanup BlendComponents involving these bottles
        if (bottleIds.Any())
        {
            var blendComponents = await _context.BlendComponents
                .Where(bc => bottleIds.Contains(bc.SourceBottleId) || bottleIds.Contains(bc.InfinityBottleId))
                .ToListAsync();
            _context.BlendComponents.RemoveRange(blendComponents);

            // 4. Identify remaining notes tied to bottles being deleted (that weren't just deleted as user notes)
            var userNoteIds = userNotes.Select(un => un.Id).ToHashSet();

            var bottleNotes = await _context.TastingNotes
                .Where(n => n.BottleId != null && bottleIds.Contains(n.BottleId.Value))
                .ToListAsync();
                
            var notesToDelete = bottleNotes.Where(bn => !userNoteIds.Contains(bn.Id)).ToList();
            _context.TastingNotes.RemoveRange(notesToDelete);
        }

        _context.Bottles.RemoveRange(bottles);

        // Cleanup: Existing Memberships & Invitations
        var memberships = await _context.CollectionMembers.Where(m => m.UserId == userId).ToListAsync();
        _context.CollectionMembers.RemoveRange(memberships);

        var invitations = await _context.CollectionInvitations.Where(i => i.InviteeEmail == user.Email).ToListAsync();
        _context.CollectionInvitations.RemoveRange(invitations);

        await _context.SaveChangesAsync();
        
        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            TempData["Message"] = $"User {user.Email} and their data have been deleted.";
        }
        else
        {
            TempData["ErrorMessage"] = "Error deleting user.";
        }

        return RedirectToPage();
    }
}
