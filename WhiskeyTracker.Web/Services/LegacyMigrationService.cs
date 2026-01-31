using Microsoft.EntityFrameworkCore;
using WhiskeyTracker.Web.Data;

namespace WhiskeyTracker.Web.Services;

public class LegacyMigrationService
{
    private readonly AppDbContext _context;

    public LegacyMigrationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task EnsureUserHasCollectionAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId)) return;

        // 1. Ensure User has a Collection
        var hasCollection = await _context.CollectionMembers.AnyAsync(m => m.UserId == userId);
        if (!hasCollection)
        {
            // Create Default Collection
            var personalCollection = new Collection { Name = "My Home Bar" };
            _context.Collections.Add(personalCollection);
            await _context.SaveChangesAsync();

            _context.CollectionMembers.Add(new CollectionMember
            {
                CollectionId = personalCollection.Id,
                UserId = userId,
                Role = CollectionRole.Owner
            });
            await _context.SaveChangesAsync();
        }

        // 2. Adopt Orphan Bottles
        // Optimization: Only check for orphans if we suspect migration is needed.
        // For now, we'll keep the check but it's isolated.
        var orphanBottles = await _context.Bottles
            .Where(b => b.UserId == userId && b.CollectionId == null)
            .ToListAsync();

        if (orphanBottles.Any())
        {
            var member = await _context.CollectionMembers
                .Where(m => m.UserId == userId)
                .OrderBy(m => m.Id)
                .FirstOrDefaultAsync();
            var myCollectionId = member?.CollectionId ?? 0;

            if (myCollectionId != 0)
            {
                foreach (var orphan in orphanBottles)
                {
                    orphan.CollectionId = myCollectionId;
                }
                await _context.SaveChangesAsync();
            }
        }
    }
}
