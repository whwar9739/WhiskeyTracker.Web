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
        Collections = await _context.Collections
            .Select(c => new CollectionViewModel
            {
                Id = c.Id,
                Name = c.Name,
                BottleCount = c.Bottles.Count,
                MemberCount = c.Members.Count,
                OwnerName = c.Members
                    .Where(m => m.Role == CollectionRole.Owner)
                    .Select(m => m.User.DisplayName ?? m.User.Email)
                    .FirstOrDefault() ?? "Unknown"
            })
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostDeleteCollectionAsync(int collectionId)
    {
        var collection = await _context.Collections
            .Include(c => c.Bottles)
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.Id == collectionId);

        if (collection == null) return NotFound();

        // Cleanup: Bottles in this collection and their dependencies
        var bottleIds = collection.Bottles.Select(b => b.Id).ToList();
        if (bottleIds.Any())
        {
            var notesToDelete = await _context.TastingNotes.Where(n => n.BottleId.HasValue && bottleIds.Contains(n.BottleId.Value)).ToListAsync();
            _context.TastingNotes.RemoveRange(notesToDelete);

            var blendsToDelete = await _context.BlendComponents.Where(bc => bottleIds.Contains(bc.SourceBottleId) || bottleIds.Contains(bc.InfinityBottleId)).ToListAsync();
            _context.BlendComponents.RemoveRange(blendsToDelete);
        }

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
