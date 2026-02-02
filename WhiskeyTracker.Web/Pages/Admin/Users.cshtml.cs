using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WhiskeyTracker.Web.Data;
using Microsoft.Extensions.Logging;

namespace WhiskeyTracker.Web.Pages.Admin;

public class UsersModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AppDbContext _context;
    private readonly ILogger<UsersModel> _logger;

    public UsersModel(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, AppDbContext context, ILogger<UsersModel> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _logger = logger;
    }

    public List<UserViewModel> Users { get; set; } = new();

    public class UserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? DisplayName { get; set; }
        public bool IsAdmin { get; set; }
    }

    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public const int PageSize = 50;

    public async Task OnGetAsync(int p = 1)
    {
        CurrentPage = p;

        // Resolve N+1 issue: Fetch all users in Admin role first
        var adminRole = await _roleManager.FindByNameAsync("Admin");
        var adminRoleId = adminRole?.Id;

        var adminUserIds = new HashSet<string>();
        if (adminRoleId != null)
        {
            var adminUserIdsList = await _context.UserRoles
                .Where(ur => ur.RoleId == adminRoleId)
                .Select(ur => ur.UserId)
                .ToListAsync();
            adminUserIds = new HashSet<string>(adminUserIdsList);
        }

        // Pagination: Count total users first
        var totalUsers = await _context.Users.CountAsync();
        TotalPages = (int)Math.Ceiling(totalUsers / (double)PageSize);

        // Fetch paginated users
        Users = await _context.Users
            .OrderBy(u => u.Email)
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .Select(u => new UserViewModel
            {
                Id = u.Id,
                Email = u.Email,
                DisplayName = u.DisplayName,
                IsAdmin = false // Placeholder
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

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. Fetch bottles owned by user to identify all dependencies
            var bottles = await _context.Bottles.Where(b => b.UserId == userId).ToListAsync();
            var bottleIds = bottles.Select(b => b.Id).ToList();

            // 2. Consolidate deletion of all related tasting notes in a single query
            var notesToDelete = await _context.TastingNotes
                .Where(n => n.UserId == userId || (n.BottleId.HasValue && bottleIds.Contains(n.BottleId.Value)))
                .ToListAsync();
            _context.TastingNotes.RemoveRange(notesToDelete);

            // 3. Cleanup BlendComponents involving these bottles
            if (bottleIds.Any())
            {
                var blendComponents = await _context.BlendComponents
                    .Where(bc => bottleIds.Contains(bc.SourceBottleId) || bottleIds.Contains(bc.InfinityBottleId))
                    .ToListAsync();
                _context.BlendComponents.RemoveRange(blendComponents);
            }

            _context.Bottles.RemoveRange(bottles);

            // Cleanup: Existing Memberships & Invitations
            var memberships = await _context.CollectionMembers.Where(m => m.UserId == userId).ToListAsync();
            _context.CollectionMembers.RemoveRange(memberships);

            var invitations = await _context.CollectionInvitations.Where(i => i.InviteeEmail == user.Email).ToListAsync();
            _context.CollectionInvitations.RemoveRange(invitations);

            // Now, delete the user. The UserStore will call SaveChangesAsync, which will
            // be part of the current transaction.
            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                await transaction.CommitAsync();
                TempData["Message"] = $"User {user.Email} and their data have been deleted.";
            }
            else
            {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = $"Error deleting user: {string.Join(", ", result.Errors.Select(e => e.Description))}";
            }
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error deleting user {UserId}", userId);
            TempData["ErrorMessage"] = "A critical error occurred while deleting user data. The operation was rolled back.";
        }

        return RedirectToPage();
    }
}
