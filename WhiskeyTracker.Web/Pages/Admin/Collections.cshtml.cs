using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WhiskeyTracker.Web.Data;

namespace WhiskeyTracker.Web.Pages.Admin;

public class CollectionsModel : PageModel
{
    private readonly AppDbContext _context;

    public CollectionsModel(AppDbContext context)
    {
        _context = context;
    }

    public List<CollectionViewModel> Collections { get; set; } = new();

    public class CollectionViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int BottleCount { get; set; }
        public int MemberCount { get; set; }
        public string? OwnerName { get; set; }
    }

    public async Task OnGetAsync()
    {
        var collections = await _context.Collections
            .Include(c => c.Bottles)
            .Include(c => c.Members)
                .ThenInclude(m => m.User)
            .ToListAsync();

        foreach (var c in collections)
        {
            var owner = c.Members.FirstOrDefault(m => m.Role == CollectionRole.Owner)?.User;
            Collections.Add(new CollectionViewModel
            {
                Id = c.Id,
                Name = c.Name,
                BottleCount = c.Bottles.Count,
                MemberCount = c.Members.Count,
                OwnerName = owner?.DisplayName ?? owner?.Email ?? "Unknown"
            });
        }
    }

    public async Task<IActionResult> OnPostDeleteCollectionAsync(int collectionId)
    {
        var collection = await _context.Collections
            .Include(c => c.Bottles)
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.Id == collectionId);

        if (collection == null) return NotFound();

        // Cleanup: Bottles in this collection
        _context.Bottles.RemoveRange(collection.Bottles);
        _context.CollectionMembers.RemoveRange(collection.Members);
        
        // Cleanup: Invitations for this collection
        var invitations = await _context.CollectionInvitations.Where(i => i.CollectionId == collectionId).ToListAsync();
        _context.CollectionInvitations.RemoveRange(invitations);

        _context.Collections.Remove(collection);
        await _context.SaveChangesAsync();

        TempData["Message"] = $"Collection '{collection.Name}' and its bottles/memberships have been deleted.";
        return RedirectToPage();
    }
}
