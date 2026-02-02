using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using WhiskeyTracker.Web.Data;

namespace WhiskeyTracker.Web.Pages.Collections;

public class CreateModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public CreateModel(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [BindProperty]
    public Collection Collection { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var userId = _userManager.GetUserId(User);
        if (userId == null) return Challenge();

        // Create the collection
        _context.Collections.Add(Collection);
        await _context.SaveChangesAsync();

        // Add the creator as the Owner
        var membership = new CollectionMember
        {
            CollectionId = Collection.Id,
            UserId = userId,
            Role = CollectionRole.Owner
        };
        _context.CollectionMembers.Add(membership);
        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}
